using System.Diagnostics;
using UnityEngine;

namespace ProjectT.Game.Player.FSM.Locomotion.States
{
    /// <summary>
    /// Locomotion Idle State
    /// 플레이어 이동 대기 상태
    /// </summary>
    public sealed class LocomotionIdleState : PlayerLocomotionStateBase
    {
        public override void Enter(PlayerFsmContext ctx)
        {
            Stop(ctx);
        }
        public override void Tick(PlayerFsmContext ctx)
        {
            PlayerController pc = ctx.Controller;
            // Dodge 상태 전환
            if (pc.DodgePressed)
            {
                pc.SetLocomotion(PlayerLocomotionStateId.Dodge);
                return;
            }
            // Move 상태 전환
            if(pc.MoveInput != Vector2.zero)
            {
                pc.SetLocomotion(PlayerLocomotionStateId.Move);
                return;
            }
        }
        public override void Exit(PlayerFsmContext ctx) { }
        private static void Stop(PlayerFsmContext ctx)
        {
            if(ctx.rb!=null) ctx.rb.velocity = Vector2.zero;
        }
    }
}
