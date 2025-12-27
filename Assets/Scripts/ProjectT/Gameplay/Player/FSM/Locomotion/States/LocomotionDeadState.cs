namespace ProjectT.Gameplay.Player.FSM.Locomotion.States
{
    /// <summary>
    /// Locomotion Dead State
    /// 플레이어 사망 상태
    /// 
    /// Step 4: State는 전이 결정만, 물리 실행은 Binder에서 처리
    /// </summary>
    public sealed class LocomotionDeadState : PlayerLocomotionStateBase
    {
        public override void Enter(PlayerFsmContext ctx)
        {
            // 물리 실행(Stop)은 Binder가 OnStateChanged에서 처리
        }
        public override void Tick(PlayerFsmContext ctx) { }
        public override void Exit(PlayerFsmContext ctx) { }
    }
}