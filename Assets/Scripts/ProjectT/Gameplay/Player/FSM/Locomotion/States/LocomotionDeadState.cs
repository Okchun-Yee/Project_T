namespace ProjectT.Game.Player.FSM.Locomotion.States
{
    /// <summary>
    /// Locomotion Dead State
    /// 플레이어 사망 상태
    /// </summary>
    public sealed class LocomotionDeadState : PlayerLocomotionStateBase
    {
        public override void Enter(PlayerFsmContext ctx)
        {
            if(ctx.Rigid != null) ctx.Rigid.velocity = UnityEngine.Vector2.zero;
        }
        public override void Tick(PlayerFsmContext ctx) { }
        public override void Exit(PlayerFsmContext ctx) { }
    }
}