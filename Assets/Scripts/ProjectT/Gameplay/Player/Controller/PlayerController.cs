using System;
using UnityEngine;
using ProjectT.Core.FSM;
using ProjectT.Gameplay.Player.FSM;
using ProjectT.Gameplay.Player.FSM.Locomotion;
using ProjectT.Gameplay.Player.FSM.Combat;
using ProjectT.Gameplay.Player.Input;
using ProjectT.Gameplay.Weapon;

namespace ProjectT.Gameplay.Player
{
    /// <summary>
    /// Player Controller
    /// 두 개의 독립적인 FSM(Locomotion, Combat)를 병렬 관리
    /// 
    /// [Architecture - Step 6]
    /// * Locomotion FSM: Idle, Move, Dodge, Hit, Dead
    /// * Combat FSM: None, Charging, Holding, Attack
    /// * Cross-FSM Coordinator: 두 FSM 간 충돌 정책 적용
    /// 
    /// 정책 우선순위: Dead > Hit > Pause > Dodge > 일반 전이
    /// </summary>
    public sealed class PlayerController : MonoBehaviour
    {
        #region PAUSE 상위 게이트
        [Header("Gate")]
        [SerializeField] private bool _isPaused;    // 일시정지 상태
        public bool IsPaused => _isPaused;          // 일시정지 상태 프로퍼티   
        #endregion

        public bool IsDead { get; private set; }    // 사망 상태
        // public bool IsHit { get; private set; }     // 피격 상태 (추후 사용 예정)

        [Header("Debug")]
        [SerializeField] private bool _logStateChanges;

        // 외부 접근용 프로퍼티 (입력 상태)
        public Vector2 MoveInput { get; private set; }
        public bool AttackPressed { get; private set; }
        public bool AttackHeld { get; private set; }
        public bool DodgePressed { get; private set; }
        
        // 무기 설정 기반 (Binder에서 설정, FSM 전이 조건으로 사용)
        public bool CanChargeAttack { get; set; }


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

        private bool _isBound = false;

        private void Awake()
        {
            BuildFsm();
        }

        private void Start()
        {
            InitializeFsm();
            CachePrevStates();
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
            // 게이트 검사
            if (!CanTickFsmThisFrame())
            {
                ClearOneFrameInput();
                return;
            }
            ApplyCrossFsmPoliciesPreTick();   // 정책 적용
            _locomotionFsm.Tick();          // 병렬 Tick
            _combatFsm.Tick();

            ClearOneFrameInput();           // 1프레임 입력 초기화
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
        /// 강제 Hit 전이 (외부 이벤트: 피격)
        /// Policy: L=Hit, C=None (Cancel:Hit)
        /// </summary>
        public void ForceHit()
        {
            if (IsDead) return;

            CancelCombatWithReason(ChargeCancelReason.Hit);
            _locomotionFsm.ChangeState(PlayerLocomotionStateId.Hit);
        }

        /// <summary>
        /// 강제 Dead 전이 (외부 이벤트: 사망)
        /// Policy: L=Dead, C=None, FSM 비활성화
        /// </summary>
        public void ForceDead()
        {
            if (IsDead) return;

            IsDead = true;

            CancelCombatWithReason(ChargeCancelReason.Dead);
            _locomotionFsm.ChangeState(PlayerLocomotionStateId.Dead);

            // FSM 비활성화
            _locomotionFsm.Deactivate();
            _combatFsm.Deactivate();
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
            if (InputManager.Instance == null) {
                Debug.LogWarning("[PC] InputManager Instance is null, cannot bind input.");
                return;
            }

            InputManager.Instance.OnMoveInput += OnMoveInput;
            InputManager.Instance.OnAttackInput += OnAttackStarted;
            InputManager.Instance.OnAttackCanceled += OnAttackCanceled;
            InputManager.Instance.OnDodgeInput += OnDodgeInput;

            _isBound = true;
#if UNITY_EDITOR
            Debug.Log("[PC] Input bound");
#endif
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
                CancelCombatWithReason(ChargeCancelReason.Dodge);
            }

            // Policy 2: Charging/Holding 중 Dodge 입력 → Combat 취소
            // (Dodge 전이 자체는 Locomotion Guard가 처리)
            if (DodgePressed && IsInCombatChargingOrHolding())
            {
                CancelCombatWithReason(ChargeCancelReason.Dodge);
            }

            // Policy 3: 입력 차단 (Dodge/Hit 중 Attack 무시)
            if (ShouldBlockAttackInput())
            {
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
                ChargingManager.Instance?.EndCharging(reason);
            }

            if (CombatState != PlayerCombatStateId.None)
            {
                _combatFsm.ChangeState(PlayerCombatStateId.None);
            }
        }
        #endregion

        private void ClearOneFrameInput()
        {
            AttackPressed = false;
            DodgePressed = false;
        }
        private void CachePrevStates()
        {
            _prevL = LocomotionState;
            _prevC = CombatState;
        }
        private void LogStateIfChanged()
        {
            if (_prevL == LocomotionState && _prevC == CombatState) return;
            Debug.Log($"[FSM] L:{LocomotionState} C:{CombatState}");
            CachePrevStates();
        }
        #region  Action Handlers
        private void OnMoveInput(Vector2 input)
        {
            MoveInput = input;
        }

        private void OnAttackStarted()
        {
            AttackPressed = true;   // 1프레임 트리거
            AttackHeld = true;      // 유지 입력
        }

        private void OnAttackCanceled()
        {
            AttackHeld = false;
        }

        private void OnDodgeInput()
        {
            DodgePressed = true;    // 1프레임 트리거
        }
        #endregion
    }
}