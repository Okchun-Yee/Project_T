using System;
using UnityEngine;
using ProjectT.Core.FSM;
using ProjectT.Game.Player.FSM;
using ProjectT.Game.Player.FSM.Locomotion;
using ProjectT.Game.Player.FSM.Combat;

namespace ProjectT.Game.Player
{
    /// <summary>
    /// Player Controller
    /// 두 개의 독립적인 FSM(이동, 전투)를 병렬 관리
    /// 입력 처리 <-> 상태 간 상호작용 조율
    /// [Architecture]
    /// * Locomotion FSM: 이동, 회피, 피격, 사망 상태 관리
    /// * Combat FSM: 공격, 차징, 홀딩 상태 관리
    /// - 두 FSM는 병렬 동작 => 특정 조건에서 서로 영향을 줌
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
        [SerializeField] private bool _logStateChanges = false;

        // 외부 접근용 프로퍼티
        public float ChargeTime { get; set; }
        public Vector2 MoveInput { get; private set; }
        public bool AttackPressed { get; private set; }
        public bool AttackHeld { get; private set; }
        public bool DodgePressed { get; private set; }
        public bool CanChargeAttack { get; set; }
        public bool IsChargeMaxReached { get; set; }
        public bool NextAttackIsCharged { get; set; }


        public PlayerLocomotionStateId LocomotionState => _locomotionFsm.CurrentStateId;    // 현재 Locomotion 상태
        public PlayerCombatStateId CombatState => _combatFsm.CurrentStateId;                // 현재 Combat 상태
        private PlayerFsmContext _ctx;  // FSM 공유 컨텍스트
        private StateMachine<PlayerLocomotionStateId, PlayerFsmContext> _locomotionFsm;
        private StateMachine<PlayerCombatStateId, PlayerFsmContext> _combatFsm;

        // Combat FSM 확장 포인트 이벤트
        public event Action AttackStarted;
        public event Action AttackEnded;
        public event Action ChargeStarted;
        public event Action ChargeReachedMax;
        public event Action ChargeCanceled;
        public event Action HoldStarted;

        // 이전 상태 로깅용
        private PlayerLocomotionStateId _prevL;
        private PlayerCombatStateId _prevC;


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
            if (InputManager.Instance == null) return;

            InputManager.Instance.OnMoveInput += OnMoveInput;
            InputManager.Instance.OnAttackInput += OnAttackStarted;
            InputManager.Instance.OnAttackCanceled += OnAttackCanceled;
            InputManager.Instance.OnDodgeInput += OnDodgeInput;
        }

        private void OnDisable()
        {
            if (InputManager.Instance == null) return;

            InputManager.Instance.OnMoveInput -= OnMoveInput;
            InputManager.Instance.OnAttackInput -= OnAttackStarted;
            InputManager.Instance.OnAttackCanceled -= OnAttackCanceled;
            InputManager.Instance.OnDodgeInput -= OnDodgeInput;
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
        /// 강제 이벤트 브릿지
        /// Locomotion->Hit 전환, Combat->None 전환
        /// </summary>
        public void ForceHit()
        {
            if (IsDead) return;

            _locomotionFsm.ChangeState(PlayerLocomotionStateId.Hit);
            _combatFsm.ChangeState(PlayerCombatStateId.None);
        }

        /// <summary>
        /// 강제 이벤트 브릿지
        /// Locomotion->Dead 전환, Combat->None 전환
        /// 두 FSM 비활성화
        /// </summary>
        public void ForceDead()
        {
            if (IsDead) return;

            IsDead = true;

            _locomotionFsm.ChangeState(PlayerLocomotionStateId.Dead);
            _combatFsm.ChangeState(PlayerCombatStateId.None);

            // FSM 비활성화
            _locomotionFsm.Deactivate();
            _combatFsm.Deactivate();
        }
        #endregion

        // Combat FSM 확장 포인트 알림 메서드
        public void NotifyAttackStarted() => AttackStarted?.Invoke();
        public void NotifyAttackEnded() => AttackEnded?.Invoke();
        public void NotifyChargeStarted() {
            Debug.Log("[FSM] ChargeStarted event fired");
            ChargeStarted?.Invoke();
        }
        public void NotifyChargeReachedMax() => ChargeReachedMax?.Invoke();
        public void NotifyChargeCanceled() => ChargeCanceled?.Invoke();
        public void NotifyHoldStarted() => HoldStarted?.Invoke();


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
        /// </summary>
        private void ApplyCrossFsmPoliciesPreTick()
        {
            // 1. 정책 1: Dodge 중에는 Combat FSM이 None 상태여야 함
            if (IsCombatBlockedByLocomotion())
            {
                CancelCombat();
            }
        }
        #endregion

        #region Cross-FSM Policies
        /// <summary>
        /// 정책 1 검사: Locomotion FSM이 Dodge 상태인지
        /// </summary>
        private bool IsCombatBlockedByLocomotion()
        {
            return _locomotionFsm.CurrentStateId == PlayerLocomotionStateId.Dodge;
        }

        /// <summary>
        /// Combat FSM 강제 취소: None 상태로 전환
        /// </summary>
        private void CancelCombat()
        {
            if (_combatFsm.CurrentStateId != PlayerCombatStateId.None)
                _combatFsm.ChangeState(PlayerCombatStateId.None);
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