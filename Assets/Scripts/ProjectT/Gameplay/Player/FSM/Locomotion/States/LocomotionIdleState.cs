namespace ProjectT.Gameplay.Player.FSM.Locomotion.States
{
    /// <summary>
    /// Locomotion Idle State
    /// 플레이어 이동 대기 상태
    /// 
    /// Step 5: 전이 규칙은 Composer Guard가 처리
    /// </summary>
    public sealed class LocomotionIdleState : PlayerLocomotionStateBase
    {
        public override void Enter(PlayerFsmContext ctx) { }
        public override void Tick(PlayerFsmContext ctx) { }
        public override void Exit(PlayerFsmContext ctx) { }
    }
}
