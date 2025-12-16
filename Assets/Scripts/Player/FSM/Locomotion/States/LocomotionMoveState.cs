using System;
using UnityEngine;

namespace ProjectT.Game.Player.FSM.Locomotion.States
{
    /// <summary>
    /// Locomotion Move State
    /// 플레이어 이동 상태
    /// </summary>
    public sealed class LocomotionMoveState : PlayerLocomotionStateBase
    {
        public override void Enter(PlayerFsmContext ctx) { }
        public override void Tick(PlayerFsmContext ctx)
        {
            PlayerController pc = ctx.Controller;
            // Dodge 상태 전환
            if (pc.DodgePressed)
            {
                pc.SetLocomotion(PlayerLocomotionStateId.Dodge);
                return;
            }
            // Idle 상태 전환
            if (pc.MoveInput == Vector2.zero)
            {
                pc.SetLocomotion(PlayerLocomotionStateId.Idle);
                return;
            }
            // 이동
            Move(ctx, pc.MoveInput, pc.MoveSpeed);
        }
        public override void Exit(PlayerFsmContext ctx)
        {
            Stop(ctx);
        }
    #region Methods
        // [TODO] 기존 이동 로직 점검 후 통합
        private static void Move(PlayerFsmContext ctx, Vector2 dir, float speed)
        {
            if(ctx.Rigid!=null)
            {
                ctx.Rigid.velocity = dir.normalized * speed;
                return;
            }
             ctx.Transform.Translate(dir * speed * Time.deltaTime);
        }
        private static void Stop(PlayerFsmContext ctx)
        {
            if(ctx.Rigid!=null) ctx.Rigid.velocity = Vector2.zero;
        }
    #endregion
    }
}
