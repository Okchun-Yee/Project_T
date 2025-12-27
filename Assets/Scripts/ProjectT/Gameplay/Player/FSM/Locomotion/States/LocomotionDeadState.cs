namespace ProjectT.Gameplay.Player.FSM.Locomotion.States
{
    /// <summary>
    /// Locomotion Dead State
    /// 플레이어 사망 상태 (종료 상태, 전이 없음)
    /// 
    /// Step 5: 강제 전이로만 진입 (Step 7에서 처리)
    /// </summary>
    public sealed class LocomotionDeadState : PlayerLocomotionStateBase
    {
        public override void Enter(PlayerFsmContext ctx) { }
        public override void Tick(PlayerFsmContext ctx) { }
        public override void Exit(PlayerFsmContext ctx) { }
    }
}