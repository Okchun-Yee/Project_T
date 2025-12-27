namespace ProjectT.Gameplay.Player.FSM.Combat.States
{
    /// <summary>
    /// Player combat none state.
    /// </summary>
    public sealed class CombatNoneState : PlayerCombatStateBase
    {
        public override void Enter(PlayerFsmContext ctx)
        {
            // SSOT: 상태 진입 시 별도 플래그 초기화 불필요
            // FSM 상태 자체가 유일한 권위
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