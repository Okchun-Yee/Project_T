using ProjectT.Core.FSM;
using ProjectT.Gameplay.Player.Controller;
using ProjectT.Gameplay.Player.FSM.Locomotion;
using UnityEngine;

namespace ProjectT.Gameplay.Player
{
    /// <summary>
    /// Locomotion FSM(Decision) → Execution 연결자
    /// - OnStateChanged 콜백 기반으로 전이 감지
    /// - 물리(Stop) 실행을 단일 지점에서 처리
    /// 
    /// Step 4 책임 분리:
    /// - State는 전이 결정만
    /// - Binder가 물리 실행(Stop) 담당
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerLocomotionFsmBinder : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController _decision;
        [SerializeField] private PlayerMovementExecution _execution;
        [SerializeField] private Rigidbody2D _rb;

        [Header("Options")]
        [SerializeField] private bool _zeroMoveInputWhenNotMoveState = true;

        private bool _isSubscribed = false;

        private void Awake()
        {
            if (_rb == null) _rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            // PlayerController의 FSM Ready 이벤트 구독
            if (_decision != null)
            {
                _decision.OnFsmBuilt += OnPlayerControllerFsmReady;
            }
            
            // 이미 PlayerController가 준비된 경우 즉시 구독
            if (_decision != null && _decision.LocomotionFsm != null)
            {
                SubscribeFsmCallback();
            }
        }

        private void OnDisable()
        {
            // PlayerController의 FSM Ready 이벤트 구독 해제
            if (_decision != null)
            {
                _decision.OnFsmBuilt -= OnPlayerControllerFsmReady;
            }
            
            UnsubscribeFsmCallback();
        }

        private void Start()
        {
            SubscribeFsmCallback();
        }

        /// <summary>
        /// PlayerController의 FSM 준비 완료 시 호출되는 콜백
        /// </summary>
        private void OnPlayerControllerFsmReady()
        {
            SubscribeFsmCallback();
        }

        private void SubscribeFsmCallback()
        {
            if (_isSubscribed || _decision == null) return;
            if (_decision.LocomotionFsm == null) return;

            _decision.LocomotionFsm.OnStateChanged += OnLocomotionStateChanged;
            _isSubscribed = true;
        }

        private void UnsubscribeFsmCallback()
        {
            if (!_isSubscribed || _decision == null || _decision.LocomotionFsm == null) return;

            _decision.LocomotionFsm.OnStateChanged -= OnLocomotionStateChanged;
            _isSubscribed = false;
        }

        private void Update()
        {
            if (_decision == null || _execution == null) return;

            // MoveInput 전달
            if (_zeroMoveInputWhenNotMoveState)
            {
                if (_decision.LocomotionState == PlayerLocomotionStateId.Move)
                    _execution.SetMoveInput(_decision.MoveInput);
                else
                    _execution.SetMoveInput(Vector2.zero);
            }
            else
            {
                _execution.SetMoveInput(_decision.MoveInput);
            }
        }

        /// <summary>
        /// Locomotion 상태 전이 시 실행 처리
        /// State가 하던 물리 조작을 여기서 단일화
        /// </summary>
        private void OnLocomotionStateChanged(StateChangedEventArgs<PlayerLocomotionStateId> args)
        {
            var prev = args.PrevStateId;
            var next = args.NextStateId;

            // Dodge 진입 시 대기 중인 Dash 실행
            if (next == PlayerLocomotionStateId.Dodge)
            {
                _execution?.ExecutePendingDash();
            }

            // Stop 정책: 이동을 멈춰야 하는 상태로 전이 시 velocity=0
            if (ShouldStopOnEnter(next) || ShouldStopOnExit(prev, next))
            {
                StopMovement();
            }
        }

        /// <summary>
        /// 진입 시 Stop이 필요한 상태 (Idle/Hit/Dead)
        /// </summary>
        private bool ShouldStopOnEnter(PlayerLocomotionStateId next)
        {
            return next == PlayerLocomotionStateId.Idle
                || next == PlayerLocomotionStateId.Hit
                || next == PlayerLocomotionStateId.Dead;
        }

        /// <summary>
        /// 이탈 시 Stop이 필요한 전이 (Move/Dodge에서 나갈 때 관성 제거)
        /// </summary>
        private bool ShouldStopOnExit(PlayerLocomotionStateId prev, PlayerLocomotionStateId next)
        {
            // Move/Dodge → Dodge는 이동 가속이 필요하므로 Stop하지 않음
            if ((prev == PlayerLocomotionStateId.Move || prev == PlayerLocomotionStateId.Dodge) 
                && next == PlayerLocomotionStateId.Dodge)
            {
                return false;
            }
            
            // Move/Dodge에서 나갈 때 (단, 이미 Stop 상태로 가는 경우 제외)
            bool exitingMovement = (prev == PlayerLocomotionStateId.Move || prev == PlayerLocomotionStateId.Dodge);
            bool enteringStop = ShouldStopOnEnter(next);
            return exitingMovement && !enteringStop; // 중복 Stop 방지
        }

        private void StopMovement()
        {
            if (_rb != null) _rb.velocity = Vector2.zero;
        }
    }
}
