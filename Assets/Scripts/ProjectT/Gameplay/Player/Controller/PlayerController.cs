using System;
using UnityEngine;
using ProjectT.Core.FSM;
using ProjectT.Gameplay.Player.FSM;
using ProjectT.Gameplay.Player.FSM.Locomotion;
using ProjectT.Gameplay.Player.FSM.Combat;
using ProjectT.Gameplay.Player.Input;
using ProjectT.Gameplay.Weapon;
using ProjectT.Gameplay.Player.Controller;
using ProjectT.Core;
using ProjectT.Core.Debug;
using ProjectT.Gameplay.Skills.Runtime;
using ProjectT.Gameplay.Skills;

namespace ProjectT.Gameplay.Player
{
    /// <summary>
    /// 강제 상태 전이 타입 (Step 7)
    /// 우선순위: Dead(1) > Pause(2) > Hit(3)
    /// Dodge는 입력 기반 정책으로 처리 (Step 6)
    /// </summary>
    public enum ForceStateType
    {
        None = 0,
        Hit = 3,
        Pause = 2,  // [미사용] Pause는 SetPaused() 게이트로만 처리됨
        Dead = 1
    }

    /// <summary>
    /// 액션 잠금 플래그 (스킬 시전 중 특정 행동 금지용)
    /// </summary>
    [Flags]
    public enum ActionLockFlags
    {
        None = 0,
        Move = 1 << 0,          // 이동 금지
        BasicAttack = 1 << 1,   // 기본 공격 금지
        Dash = 1 << 2,          // 대쉬 금지
        Skill = 1 << 3          // 스킬 금지 (선택)
    }

    /// <summary>
    /// Player Controller
    /// 두 개의 독립적인 FSM(Locomotion, Combat)를 병렬 관리
    /// 
    /// * Locomotion FSM: Idle, Move, Dodge, Hit, Dead
    /// * Combat FSM: None, Charging, Holding, Attack
    /// * Cross-FSM Coordinator: 두 FSM 간 충돌 정책 적용
    /// * Force State: 강제 전이 단일 진입점 (ApplyForceState)
    /// 
    /// 강제 상태 우선순위: Dead > Pause > Hit > Dodge
    /// </summary>
    public sealed class PlayerController : Singleton<PlayerController>
    {
        // Fired when FSMs have been built and initialized (CombatFsm/LocomotionFsm ready)
        public event Action OnFsmBuilt;

        #region Gate
        [Header("Gate")]
        [SerializeField] private bool _isPaused;    // 일시정지 상태
        public bool IsPaused => _isPaused;          // 일시정지 상태 프로퍼티   
        public bool IsDead { get; private set; }    // 사망 상태
        #endregion
        // public bool IsHit { get; private set; }     // 피격 상태 (추후 사용 예정)

        [Header("Debug")]
        [SerializeField] private bool _logStateChanges;
        [Header("Combat Roots")]
        [SerializeField] private Transform _combatRoot;
        [SerializeField] private Transform _spinHubRoot; // Player/Combat/SpinHub

        private SkillExecutionContext _skillCtx;

        // 외부 접근용 프로퍼티 (입력 상태)
        public Vector2 MoveInput { get; private set; }
        public bool AttackPressed { get; private set; }
        public bool AttackHeld { get; private set; }
        public bool DodgePressed { get; private set; }

        // 무기 설정 기반 (Binder에서 설정, FSM 전이 조건으로 사용)
        public bool CanChargeAttack { get; set; }

        // 액션 잠금 시스템 (스킬 시전 중 특정 행동 금지)
        private ActionLockFlags _lockedActions = ActionLockFlags.None;
        private float _actionLockTime = 0f;
        private ActionLockFlags _transitionLockedActions = ActionLockFlags.None;

        public PlayerLocomotionStateId LocomotionState => _locomotionFsm.CurrentStateId;    // 현재 Locomotion 상태
        public PlayerCombatStateId CombatState => _combatFsm.CurrentStateId;                // 현재 Combat 상태

        /// <summary>
        /// Combat FSM 인스턴스 (Binder가 OnStateChanged 콜백을 구독하기 위해 노출)
        /// </summary>
        public StateMachine<PlayerCombatStateId, PlayerFsmContext> CombatFsm => _combatFsm;

        /// <summary>
        /// Locomotion FSM 인스턴스 (Binder가 OnStateChanged 콜백을 구독하기 위해 노출)
        /// </summary>
        public StateMachine<PlayerLocomotionStateId, PlayerFsmContext> LocomotionFsm => _locomotionFsm;

        private PlayerFsmContext _ctx;  // FSM 공유 컨텍스트
        private StateMachine<PlayerLocomotionStateId, PlayerFsmContext> _locomotionFsm;
        private StateMachine<PlayerCombatStateId, PlayerFsmContext> _combatFsm;

        // 이벤트는 Binder(PlayerCombatFsmBinder)가 소유
        // PlayerController는 FSM 상태만 관리

        // 이전 상태 로깅용
        private PlayerLocomotionStateId _prevL;
        private PlayerCombatStateId _prevC;
        private bool _wasTickSkipped;

        private bool _isBound = false;

        private void Start()
        {
            BuildFsm();
            InitializeFsm();
            // Notify listeners that FSMs are ready (Binder may be waiting)
            _skillCtx = new SkillExecutionContext(transform, _spinHubRoot);
            OnFsmBuilt?.Invoke();
        }
        private void OnEnable()
        {
            InputManager.Ready += TryBindInput;

            // InputManager가 이미 준비된 경우 즉시 바인딩 시도
            if (InputManager.Instance != null)
            {
                TryBindInput();
            }
        }

        private void OnDisable()
        {
            InputManager.Ready -= TryBindInput;
            UnbindInput();
            _isBound = false;
        }

        /// <summary>
        /// 입력 수집(임시) -> 게이트 검사 -> 정책 적용 -> 병렬 Tick -> 1프레임 입력 초기화
        /// </summary>
        private void Update()
        {
            // 0. 액션 잠금 타이머 감소
            if (_actionLockTime > 0f)
            {
                _actionLockTime -= Time.deltaTime;
                if (_actionLockTime <= 0f)
                {
                    _lockedActions = ActionLockFlags.None;
                }
            }

            // 게이트 검사
            if (!CanTickFsmThisFrame())
            {
                LogTickSkippedIfChanged();
                ConsumeOneFrameInputs();
                return;
            }
            ApplyCrossFsmPoliciesPreTick();   // 정책 적용
            _locomotionFsm.Tick();          // 병렬 Tick
            _combatFsm.Tick();
            LogTickIfStateChanged();

            ConsumeOneFrameInputs();           // 1프레임 입력 초기화 (Pressed/Released 소비)
        }
        private void LateUpdate()
        {
            if (!_logStateChanges) return;
            LogStateIfChanged();
        }

        #region Bridge/Gate Policies
        /// <summary>
        /// Pause 상위 게이트
        /// Pause면 FSM Tick을 수행하지 않는다.
        /// </summary>
        public void SetPaused(bool paused)
        {
            _isPaused = paused;
            Time.timeScale = paused ? 0f : 1f;  // 게임 정지/재개
            
            // Pause 진입 시 Charging/Holding 취소
            if (paused && IsInCombatChargingOrHolding())
            {
                CancelCombatWithReason(ChargeCancelReason.Pause);
            }
        }

        /// <summary>
        /// Locomotion FSM 상태 전환 헬퍼
        /// </summary>
        public void SetLocomotion(PlayerLocomotionStateId id)
        {
            _locomotionFsm.ChangeState(id);
        }

        /// <summary>
        /// Combat FSM 상태 전환 헬퍼
        /// </summary>
        public void SetCombat(PlayerCombatStateId id)
        {
            _combatFsm.ChangeState(id);
        }

        /// <summary>
        /// 강제 Hit 전이 (외부 API)
        /// </summary>
        public void ForceHit() => ApplyForceState(ForceStateType.Hit);

        /// <summary>
        /// 강제 Dead 전이 (외부 API)
        /// </summary>
        public void ForceDead() => ApplyForceState(ForceStateType.Dead);
        #endregion

        #region Request
        /// <summary>
        /// 대시 요청 단일 진입점 (외부 스킬/시스템용)
        /// Execution에 Context를 위임하고 FSM 전이 요청
        /// </summary>
        public void RequestDash(Controller.DashContext context)
        {
            // 1. Execution에 pending 설정
            PlayerMovementExecution execution = GetComponent<PlayerMovementExecution>();
            if (execution != null)
            {
                execution.SetPendingDash(context);
            }

            // 2. FSM 전이 요청
            SetLocomotion(PlayerLocomotionStateId.Dodge);
        }

        /// <summary>
        /// 키보드 입력용 Dodge (내부 전용)
        /// Binder에서 호출될 수 있도록 유지
        /// </summary>
        public void RequestKeyboardDodge()
        {
            PlayerMovementExecution execution = GetComponent<PlayerMovementExecution>();
            if (execution == null) return;

            RequestDash(Controller.DashContext.CreateForDodge(
                direction: execution.GetDirection(),
                force: execution.DodgeForce,
                duration: execution.DodgeDuration
            ));
        }

        public void ExecuteSkill(BaseSkill skill)
        {
            if (skill == null) return;
            skill.Execute(_skillCtx);
        }
        #endregion

        #region Force State
        /// <summary>
        /// 강제 상태 전이 단일 진입점
        /// - 상태 기반 Gate 조건: Dead/Pause/Hit 상태에 따라 요청을 제한 (숫자 우선순위 비교 아님)
        /// - 동반 조치: Combat 취소, FSM 비활성화 등
        /// - 상태 전이: Locomotion FSM 전이
        /// - Hit 진입은 ForceHit() 단일 경로로만 허용 (PreTick 정책 아님)
        /// </summary>
        private void ApplyForceState(ForceStateType type)
        {
            // 1. 우선순위 검사
            if (!CanApplyForceState(type)) return;

            // 2. Cancel reason 매핑
            ChargeCancelReason reason = MapToCancelReason(type);

            // 3. 동반 조치: Combat 취소
            CancelCombatWithReason(reason);

            // 4. 상태별 전이 및 추가 조치
            switch (type)
            {
                case ForceStateType.Hit:
                    _locomotionFsm.ChangeState(PlayerLocomotionStateId.Hit);
                    break;

                case ForceStateType.Dead:
                    IsDead = true;
                    _locomotionFsm.ChangeState(PlayerLocomotionStateId.Dead);
                    _locomotionFsm.Deactivate();
                    _combatFsm.Deactivate();
                    break;
            }
        }

        /// <summary>
        /// 강제 상태 적용 가능 여부 (상태 기반 Gate 조건)
        /// - Dead면 모든 요청 무시
        /// - Pause 중에는 Dead만 허용
        /// - Hit 중 Hit 재요청 무시
        /// </summary>
        private bool CanApplyForceState(ForceStateType type)
        {
            // Dead면 Dead만 허용 (이미 Dead면 무시)
            if (IsDead) return false;

            // Pause 중에는 Dead만 허용
            if (_isPaused && type != ForceStateType.Dead) return false;

            // Hit 중 Hit 요청은 무시 (중복 방지)
            if (IsHit() && type == ForceStateType.Hit) return false;

            return true;
        }

        /// <summary>
        /// ForceStateType → ChargeCancelReason 매핑
        /// </summary>
        private ChargeCancelReason MapToCancelReason(ForceStateType type)
        {
            return type switch
            {
                ForceStateType.Dead => ChargeCancelReason.Dead,
                ForceStateType.Hit => ChargeCancelReason.Hit,
                ForceStateType.Pause => ChargeCancelReason.Pause,
                _ => ChargeCancelReason.Other
            };
        }
        #endregion

        // 이벤트 발행은 Binder(PlayerCombatFsmBinder)가 담당
        // State에서 직접 Notify 호출하지 않음

        #region 내부 구성/구동
        /// <summary>
        /// FSM 빌드 (컨텍스트 + FSM 생성/등록)
        /// </summary>
        private void BuildFsm()
        {
            // 공통 컨텍스트 생성: 모든 State가 이를 통해 player controller에 접근 가능
            _ctx = new PlayerFsmContext(this);

            // Composer를 통해 FSM 생성 및 상태 등록(State 등록, 전이 규칙 설정)
            _locomotionFsm = PlayerLocomotionFsmComposer.Create();
            _combatFsm = PlayerCombatFsmComposer.Create();
        }

        /// <summary>
        /// FSM 초기화 (초기 상태 설정)
        /// </summary>
        private void InitializeFsm()
        {
            // 초기 locomotion: Idle
            _locomotionFsm.Initialize(_ctx, PlayerLocomotionStateId.Idle);
            // 초기 Combat: None
            _combatFsm.Initialize(_ctx, PlayerCombatStateId.None);
        }
        private void TryBindInput()
        {
            if (_isBound) return;
            if (InputManager.Instance == null)
            {
                Debug.LogWarning("[PC] InputManager Instance is null, cannot bind input.");
                return;
            }

            InputManager.Instance.OnMoveInput += OnMoveInput;
            InputManager.Instance.OnAttackInput += OnAttackStarted;
            InputManager.Instance.OnAttackCanceled += OnAttackCanceled;
            InputManager.Instance.OnDodgeInput += OnDodgeInput;

            _isBound = true;
        }

        private void UnbindInput()
        {
            if (InputManager.Instance == null) return;

            InputManager.Instance.OnMoveInput -= OnMoveInput;
            InputManager.Instance.OnAttackInput -= OnAttackStarted;
            InputManager.Instance.OnAttackCanceled -= OnAttackCanceled;
            InputManager.Instance.OnDodgeInput -= OnDodgeInput;
        }


        /// <summary>
        /// FSM Tick 가능 여부 검사
        /// Pause, Dead 게이트 검사
        /// </summary>
        private bool CanTickFsmThisFrame()
        {
            if (_isPaused) return false;
            if (IsDead) return false;
            return true;
        }

        /// <summary>
        /// FSM 간 브릿지 정책 적용
        /// - 두 FSM 간 입력/상태 충돌 방지
        /// - 강제 취소/차단 같은 상위 정책 반영
        /// 
        /// [Step 6] Cross-FSM Policy 적용 순서:
        /// 1. Dodge 진입 시 Combat 취소
        /// 2. Charging/Holding 중 Dodge 입력 → Combat 취소 (Dodge 전이는 Guard가 처리)
        /// 3. 입력 차단 (Dodge/Hit 중 Attack 무시)
        /// </summary>
        private void ApplyCrossFsmPoliciesPreTick()
        {
            // Policy 1: Dodge 상태면 Combat 강제 취소
            if (IsDodging() && CombatState != PlayerCombatStateId.None)
            {
                DevLog.Log(DevLogChannels.PlayerFsm, "Policy: Dodge state cancels combat");
                CancelCombatWithReason(ChargeCancelReason.Dodge);
            }

            // Policy 2: Charging/Holding 중 Dodge 입력 → Combat 취소
            // (Dodge 전이 자체는 Locomotion Guard가 처리)
            if (DodgePressed && IsInCombatChargingOrHolding())
            {
                DevLog.Log(DevLogChannels.PlayerFsm, "Policy: Dodge input cancels charging/holding");
                CancelCombatWithReason(ChargeCancelReason.Dodge);
            }

            // Policy 3: 입력 차단 (Dodge/Hit 중 Attack 무시)
            if (ShouldBlockAttackInput())
            {
                if (AttackPressed || AttackHeld)
                {
                    DevLog.Log(DevLogChannels.PlayerFsm, "Policy: Attack input blocked");
                }
                AttackPressed = false;
                AttackHeld = false;
            }
        }
        #endregion

        #region Cross-FSM Coordinator (Step 6)

        /// <summary>
        /// Locomotion이 Dodge 상태인지
        /// </summary>
        private bool IsDodging() => LocomotionState == PlayerLocomotionStateId.Dodge;

        /// <summary>
        /// Locomotion이 Hit 상태인지
        /// </summary>
        private bool IsHit() => LocomotionState == PlayerLocomotionStateId.Hit;

        /// <summary>
        /// Combat이 Charging 또는 Holding 상태인지
        /// </summary>
        private bool IsInCombatChargingOrHolding()
        {
            return CombatState == PlayerCombatStateId.Charging
                || CombatState == PlayerCombatStateId.Holding;
        }

        /// <summary>
        /// Attack 입력을 차단해야 하는지
        /// Dodge/Hit 중에는 공격 입력 무시
        /// </summary>
        private bool ShouldBlockAttackInput()
        {
            return IsDodging() || IsHit();
        }

        /// <summary>
        /// Combat FSM 취소 (사유 포함)
        /// ChargingManager에 Cancel reason 전달 후 None 전이
        /// </summary>
        private void CancelCombatWithReason(ChargeCancelReason reason)
        {
            // Charging/Holding 상태면 ChargingManager에 취소 사유 전달
            if (IsInCombatChargingOrHolding())
            {
                // Step 8: ChargingManager Singleton 직접 접근 (DI 경계 미적용 상태)
                var cm = ChargingManager.Instance;
                cm?.EndCharging(reason);
            }

            // 인터럽트 시 무기 회전 잠금 해제 (안전장치)
            ActiveWeapon.Instance?
                .GetComponent<ProjectT.Gameplay.Weapon.WeaponRotationLockController>()
                ?.UnlockZ();

            if (CombatState != PlayerCombatStateId.None)
            {
                _combatFsm.ChangeState(PlayerCombatStateId.None);
            }
        }
        #endregion

        /// <summary>
        /// 1프레임 입력 소비 규칙
        /// - Pressed/Released: 이번 프레임에만 유효하므로 Update 마지막에 소비
        /// - Held는 별도(입력 취소 이벤트에서만 변경)
        /// </summary>
        private void ConsumeOneFrameInputs()
        {
            if (AttackPressed || DodgePressed)
            {
                DevLog.Log(DevLogChannels.PlayerFsm, $"Consume inputs (AttackPressed:{AttackPressed}, DodgePressed:{DodgePressed})");
            }
            AttackPressed = false; // 1프레임 트리거 소비
            DodgePressed = false;  // 1프레임 트리거 소비
        }
        private void UpdatePrevStatesToCurrent()
        {
            _prevL = LocomotionState;
            _prevC = CombatState;
        }
        private void LogStateIfChanged()
        {
            if (_prevL == LocomotionState && _prevC == CombatState) return;
            // Debug.Log($"[FSM] L:{LocomotionState} C:{CombatState}");  // 상태 전이 확인용 로그
            UpdatePrevStatesToCurrent();
        }
        private void LogTickSkippedIfChanged()
        {
            bool isSkipped = _isPaused || IsDead;
            if (isSkipped == _wasTickSkipped) return;
            if (isSkipped)
            {
                DevLog.Log(DevLogChannels.PlayerFsm, $"Tick skipped (paused:{_isPaused} dead:{IsDead})");
            }
            else
            {
                DevLog.Log(DevLogChannels.PlayerFsm, "Tick resumed");
            }
            _wasTickSkipped = isSkipped;
        }

        private void LogTickIfStateChanged()
        {
            if (_prevL == LocomotionState && _prevC == CombatState) return;
            DevLog.Log(DevLogChannels.PlayerFsm, $"Tick begin L:{_prevL} C:{_prevC}");
            DevLog.Log(DevLogChannels.PlayerFsm, $"Tick end L:{LocomotionState} C:{CombatState}");
            UpdatePrevStatesToCurrent();
        }
        #region  Action Handlers
        private void OnMoveInput(Vector2 input)
        {
            MoveInput = input;
        }

        private void OnAttackStarted()
        {
            if (IsActionLocked(ActionLockFlags.BasicAttack)) return;

            AttackPressed = true;   // 1프레임 트리거
            AttackHeld = true;      // 유지 입력
        }

        private void OnAttackCanceled()
        {
            AttackHeld = false;
        }

        private void OnDodgeInput()
        {
            if (IsActionLocked(ActionLockFlags.Dash)) return;

            DodgePressed = true;    // 1프레임 트리거
            RequestKeyboardDodge(); // Context 설정
        }
        #endregion

        #region Action Lock API
        /// <summary>
        /// 특정 액션들을 duration 동안 잠금 (스킬 시전 중 사용)
        /// </summary>
        public void LockActions(ActionLockFlags flags, float duration)
        {
            _lockedActions |= flags;
            _actionLockTime = Mathf.Max(_actionLockTime, duration);
        }

        /// <summary>
        /// 특정 액션이 잠겨있는지 확인
        /// </summary>
        public bool IsActionLocked(ActionLockFlags flag)
        {
            return ((_lockedActions | _transitionLockedActions) & flag) != 0;
        }
        
        /// <summary>
        /// 씬 전환 등 시스템 레벨 잠금 (명시적 해제 필요)
        /// </summary>
        public void AcquireTransitionLock(ActionLockFlags flags)
        {
            _transitionLockedActions |= flags;
        }

        /// <summary>
        /// 씬 전환 잠금 해제
        /// </summary>
        public void ReleaseTransitionLock(ActionLockFlags flags)
        {
            _transitionLockedActions &= ~flags;
        }
        #endregion
    }
}
