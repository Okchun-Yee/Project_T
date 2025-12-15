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
        [Header("Gate")]
    #region PAUSE 상위 게이트
        [SerializeField] private bool _isPaused;    // 일시정지 상태
        public bool IsPaused => _isPaused;          // 일시정지 상태 프로퍼티   
    #endregion

        public bool IsDead { get; private set; }    // 사망 상태
        // public bool IsHit { get; private set; }     // 피격 상태 (추후 사용 예정)

    #region Input (임시)
        // [TODO] InputManager 시스템으로 교체 예정
        public Vector2 MoveInput { get; private set; }
        public bool AttackPressed { get; private set; }
        public bool AttackHeld { get; private set; }
        public bool DodgePressed { get; private set; }
    #endregion

        public PlayerLocomotionStateId LocomotionState => _locomotionFsm.CurrentStateId;    // 현재 Locomotion 상태
        public PlayerCombatStateId CombatState => _combatFsm.CurrentStateId;                // 현재 Combat 상태
        private PlayerFsmContext _ctx;  // FSM 공유 컨텍스트
        private StateMachine<PlayerLocomotionStateId, PlayerFsmContext> _locomotionFsm;
        private StateMachine<PlayerCombatStateId, PlayerFsmContext> _combatFsm;

        private void Awake()
        {
            BuildFsm();
        }

        private void Start()
        {
            InitializeFsm();
        }

        /// <summary>
        /// 입력 수집(임시) -> 게이트 검사 -> 정책 적용 -> 병렬 Tick -> 1프레임 입력 초기화
        /// </summary>
        private void Update()
        {
            // [TODO] InputManager 시스템으로 교체 예정
            PollDummyInput();               // 입력 수집 (임시)
            // 게이트 검사
            if (!ShouldTickFsm())
            {
                ClearOneFrameInput();
                return;
            }
            ApplyBridgePoliciesPreTick();   // 정책 적용
            _locomotionFsm.Tick();          // 병렬 Tick
            _combatFsm.Tick();

            ClearOneFrameInput();           // 1프레임 입력 초기화
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
        private bool ShouldTickFsm()
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
        private void ApplyBridgePoliciesPreTick()
        {
            // 1. 정책 1: Dodge 중에는 Combat FSM이 None 상태여야 함
            bool isDodging = (_locomotionFsm.CurrentStateId == PlayerLocomotionStateId.Dodge);

            if (isDodging && _combatFsm.CurrentStateId != PlayerCombatStateId.None)
                _combatFsm.ChangeState(PlayerCombatStateId.None);
        }
    #endregion

    #region Input (임시)
        // [TODO] InputManager 시스템으로 교체 예정
        private void PollDummyInput()
        {
            float x = 0f;
            float y = 0f;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y -= 1f;

            MoveInput = new Vector2(x, y).normalized;

            AttackPressed = Input.GetKeyDown(KeyCode.J);
            AttackHeld = Input.GetKey(KeyCode.J);
            DodgePressed = Input.GetKeyDown(KeyCode.Space);

            if (Input.GetKeyDown(KeyCode.P)) SetPaused(!IsPaused);
            if (Input.GetKeyDown(KeyCode.H)) ForceHit();
            if (Input.GetKeyDown(KeyCode.K)) ForceDead();
        }

        private void ClearOneFrameInput()
        {
            AttackPressed = false;
            DodgePressed = false;
        }
    #endregion
    }
}