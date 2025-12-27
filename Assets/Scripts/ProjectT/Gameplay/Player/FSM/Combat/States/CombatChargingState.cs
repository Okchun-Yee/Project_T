namespace ProjectT.Gameplay.Player.FSM.Combat.States
{
    /// <summary>
    /// Player combat charging state.
    /// Step 5: 전이 로직은 Composer에서 중앙 관리
    /// </summary>
    public sealed class CombatChargingState : PlayerCombatStateBase
    {
        public override void Enter(PlayerFsmContext ctx) { }
        public override void Tick(PlayerFsmContext ctx)
        {
            // 전이 규칙은 Composer에서 Guard로 처리
        }
        public override void Exit(PlayerFsmContext ctx) { }
    }
}