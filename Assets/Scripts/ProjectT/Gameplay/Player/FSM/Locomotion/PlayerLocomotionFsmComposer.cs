using ProjectT.Core.FSM;
using ProjectT.Gameplay.Player.FSM.Locomotion.States;

namespace ProjectT.Gameplay.Player.FSM.Locomotion
{
    /// <summary>
    /// Player locomotion FSM composer.
    /// 플레이어 이동 FSM 등록 담당 클래스.
    /// </summary>
    public static class PlayerLocomotionFsmComposer
    {
        public static StateMachine<PlayerLocomotionStateId, PlayerFsmContext> Create()
        {
            var fsm = new StateMachine<PlayerLocomotionStateId, PlayerFsmContext>();

            fsm.RegisterState(PlayerLocomotionStateId.Idle,  new LocomotionIdleState());
            fsm.RegisterState(PlayerLocomotionStateId.Move,  new LocomotionMoveState());
            fsm.RegisterState(PlayerLocomotionStateId.Dodge, new LocomotionDodgeState());
            fsm.RegisterState(PlayerLocomotionStateId.Hit,   new LocomotionHitState());
            fsm.RegisterState(PlayerLocomotionStateId.Dead,  new LocomotionDeadState());

            return fsm;
        }
    }
}
