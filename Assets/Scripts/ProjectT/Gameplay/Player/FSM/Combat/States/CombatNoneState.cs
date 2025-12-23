namespace ProjectT.Gameplay.Player.FSM.Combat.States
{
    /// <summary>
    /// Player combat none state.
    /// </summary>
    public sealed class CombatNoneState : PlayerCombatStateBase
    {
        public override void Enter(PlayerFsmContext ctx)
        {
            ctx.Controller.ChargeTime = 0f;
            ctx.Controller.IsChargeMaxReached = false; // 최대 차징 도달 플래그 초기화
        }
        public override void Tick(PlayerFsmContext ctx)
        {
            PlayerController pc = ctx.Controller;
            if (pc.AttackPressed)
            {
                if (pc.CanChargeAttack)
                    pc.SetCombat(PlayerCombatStateId.Charging);
                else
                    pc.SetCombat(PlayerCombatStateId.Attack);

                return;
            }
        }
        public override void Exit(PlayerFsmContext ctx) { }
    }
}