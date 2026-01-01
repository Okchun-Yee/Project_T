using ProjectT.Core.FSM;
using ProjectT.Gameplay.Player.FSM.Locomotion.States;
using UnityEngine;

namespace ProjectT.Gameplay.Player.FSM.Locomotion
{
    /// <summary>
    /// Locomotion FSM Composer
    /// - 상태 등록
    /// - 전이 규칙 등록 (Guard 기반)
    /// 
    /// Step 5: 전이 규칙 중앙화
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

            RegisterTransitions(fsm);

            return fsm;
        }

        /// <summary>
        /// Locomotion 전이 규칙 (우선순위 순서로 등록)
        /// </summary>
        private static void RegisterTransitions(StateMachine<PlayerLocomotionStateId, PlayerFsmContext> fsm)
        {
            // Idle → Dodge: DodgePressed (우선순위 높음)
            fsm.AddTransition(PlayerLocomotionStateId.Idle, PlayerLocomotionStateId.Dodge,
                ctx => ctx.Controller.DodgePressed);

            // Idle → Move: HasMoveInput
            fsm.AddTransition(PlayerLocomotionStateId.Idle, PlayerLocomotionStateId.Move,
                ctx => ctx.Controller.MoveInput != Vector2.zero);

            // Move → Dodge: DodgePressed (우선순위 높음)
            fsm.AddTransition(PlayerLocomotionStateId.Move, PlayerLocomotionStateId.Dodge,
                ctx => ctx.Controller.DodgePressed);

            // Move → Idle: NoMoveInput
            fsm.AddTransition(PlayerLocomotionStateId.Move, PlayerLocomotionStateId.Idle,
                ctx => ctx.Controller.MoveInput == Vector2.zero);

            // Dodge → Idle: 타이머 종료 (State 내부에서 처리)
            // Hit → Idle/Move: 타이머 종료 (State 내부에서 처리)
            // ForceHit, ForceDead: 외부 강제 전이 (Step7)
        }
    }
}
