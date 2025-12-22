using ProjectT.Gameplay.Player.FSM.Locomotion;
using UnityEngine;

namespace ProjectT.Gameplay.Player
{
    /// <summary>
    /// Locomotion FSM(Decision) → Legacy Controller(Execution) 연결자
    /// - MoveInput을 실행 계층에 전달
    /// - DodgeState 진입 순간에만 TryDodge 호출
    /// </summary>
    public sealed class PlayerLocomotionFsmBinder : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController _decision;            // FSM PlayerController
        [SerializeField] private PlayerLegacyController _execution;     // Legacy execution controller

        [Header("Options")]
        [SerializeField] private bool _zeroMoveInputWhenNotMoveState = true;

        private PlayerLocomotionStateId _prevLocomotionState;

        private void Awake()
        {
            _prevLocomotionState = default;
            
        }

        private void Start()
        {
            if (_decision != null)
                _prevLocomotionState = _decision.LocomotionState;
        }
        private void Update()
        {
            if (_decision == null || _execution == null)
                return;

            // 1) MoveInput 전달 (언제 전달할지는 정책 선택)
            if (_zeroMoveInputWhenNotMoveState)
            {
                // Move 상태일 때만 입력 전달, 그 외에는 0으로 고정
                if (_decision.LocomotionState == PlayerLocomotionStateId.Move)
                    _execution.SetMoveInput(_decision.MoveInput);
                else
                    _execution.SetMoveInput(Vector2.zero);
            }
            else
            {
                // 항상 전달 (레거시 쪽에서 알아서 처리)
                _execution.SetMoveInput(_decision.MoveInput);
            }

            // 2) DodgeState "진입 순간"에만 TryDodge 1회 호출
            PlayerLocomotionStateId current = _decision.LocomotionState;
            if (current == PlayerLocomotionStateId.Dodge && _prevLocomotionState != PlayerLocomotionStateId.Dodge)
            {
                _execution.TryDodge();
            }
 
            _prevLocomotionState = current;
        }
    }
}
