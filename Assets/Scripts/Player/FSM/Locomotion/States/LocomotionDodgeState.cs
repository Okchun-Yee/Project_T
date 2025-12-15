using System;
using UnityEngine;

namespace ProjectT.Game.Player.FSM.Locomotion.States
{
    /// <summary>
    /// Locomotion Dodge State
    /// 플레이어 이동 회피 상태
    /// </summary>
    public sealed class LocomotionDodgeState : PlayerLocomotionStateBase
    {
        private float _timeLeft;
        private Vector2 _dir;
        
        public override void Enter(PlayerFsmContext ctx)
        {
            PlayerController pc = ctx.Controller;
            _timeLeft = pc.DodgeDuration;
            _dir = pc.MoveInput != Vector2.zero ? pc.MoveInput : Vector2.right;

            ApplyDodgeVelocity(ctx, _dir, pc.DodgeSpeed);
        }
        public override void Tick(PlayerFsmContext ctx)
        {
            _timeLeft -= Time.deltaTime;

            if (_timeLeft <= 0f)
            {
                ctx.Controller.SetLocomotion(PlayerLocomotionStateId.Idle);
                return;
            }

            ApplyDodgeVelocity(ctx, _dir, ctx.Controller.DodgeSpeed);
        }

        public override void Exit(PlayerFsmContext ctx)
        {
            if(ctx.rb!=null) ctx.rb.velocity = Vector2.zero;
        }
        // [TODO] 기존 Dodge 로직 점검 후 통합
        private static void ApplyDodgeVelocity(PlayerFsmContext ctx, Vector2 dir, float speed)
        {
            if(ctx.rb!=null)
            {
                ctx.rb.velocity = dir.normalized * speed;
                return;
            }
            ctx.Transform.Translate(dir * speed * Time.deltaTime);
        }
    }
}