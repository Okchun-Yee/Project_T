namespace ProjectT.Gameplay.Player.FSM.Combat.States
{
    /// <summary>
    /// Player combat holding state.
    /// 이벤트 발행은 Binder가 담당 (State는 결정만)
    /// </summary>
    public sealed class CombatHoldingState : PlayerCombatStateBase
    {
        public override void Enter(PlayerFsmContext ctx)
        {
            // 이벤트 발행은 Binder가 전이 감지로 처리 (ChargeReachedMax)
        }
        public override void Tick(PlayerFsmContext ctx)
        {
            PlayerController pc = ctx.Controller;
            if(pc.AttackHeld) return;

            pc.SetCombat(PlayerCombatStateId.Attack);
        }
        public override void Exit(PlayerFsmContext ctx) { }
    }
}