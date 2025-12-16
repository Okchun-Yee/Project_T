namespace ProjectT.Game.Player.FSM.Combat.States
{
    /// <summary>
    /// Player combat none state.
    /// </summary>
    public sealed class CombatNoneState : PlayerCombatStateBase
    {
        public override void Enter(PlayerFsmContext ctx)
        {
            ctx.Controller.ChargeTime = 0f;
        }
        public override void Tick(PlayerFsmContext ctx)
        {
            PlayerController pc = ctx.Controller;
            if(pc.AttackPressed)
            {
                pc.SetCombat(PlayerCombatStateId.Attack);
                return;
            }
            if (pc.AttackHeld)
            {
                pc.SetCombat(PlayerCombatStateId.Charging);
                return;
            }
        }
        public override void Exit(PlayerFsmContext ctx) { }
    }
}