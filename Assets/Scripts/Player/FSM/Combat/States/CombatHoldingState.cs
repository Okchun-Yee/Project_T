namespace ProjectT.Game.Player.FSM.Combat.States
{
    /// <summary>
    /// Player combat holding state.
    /// </summary>
    public sealed class CombatHoldingState : PlayerCombatStateBase
    {
        public override void Enter(PlayerFsmContext ctx) { }
        public override void Tick(PlayerFsmContext ctx)
        {
            PlayerController pc = ctx.Controller;
            if(pc.AttackHeld) return;

            pc.SetCombat(PlayerCombatStateId.Attack);
        }
        public override void Exit(PlayerFsmContext ctx) { }
    }
}