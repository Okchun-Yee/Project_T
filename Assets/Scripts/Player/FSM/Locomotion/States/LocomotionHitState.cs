using UnityEngine;

namespace ProjectT.Game.Player.FSM.Locomotion.States
{
    /// <summary>
    /// Locomotion Hit State
    /// 플레이어 피격 상태
    /// </summary>
    public sealed class LocomotionHitState : PlayerLocomotionStateBase
    {
        private float _timeLeft;

        [SerializeField] private float _hitStunDuration = 0.2f;
        public float HitStunDuration => _hitStunDuration;

        public override void Enter(PlayerFsmContext ctx)
        {
            _timeLeft = HitStunDuration;
            if(ctx.Rigid != null) ctx.Rigid.velocity = Vector2.zero;
        }
        public override void Tick(PlayerFsmContext ctx)
        {
            _timeLeft -= Time.deltaTime;

            if(_timeLeft > 0f) return;

            PlayerController pc = ctx.Controller;
            pc.SetLocomotion(pc.MoveInput != Vector2.zero 
                ? PlayerLocomotionStateId.Move 
                : PlayerLocomotionStateId.Idle);
        }
        public override void Exit(PlayerFsmContext ctx) { }
    }
}