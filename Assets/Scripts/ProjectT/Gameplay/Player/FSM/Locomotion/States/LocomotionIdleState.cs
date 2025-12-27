namespace ProjectT.Gameplay.Player.FSM.Locomotion.States
{
    /// <summary>
    /// Locomotion Idle State
    /// 플레이어 이동 대기 상태
    /// 
    /// Step 4: State는 전이 결정만, 물리 실행은 Binder에서 처리
    /// </summary>
    public sealed class LocomotionIdleState : PlayerLocomotionStateBase
    {
        public override void Enter(PlayerFsmContext ctx)
        {
            // 물리 실행(Stop)은 Binder가 OnStateChanged에서 처리
        }
        public override void Tick(PlayerFsmContext ctx)
        {
            PlayerController pc = ctx.Controller;
            // Dodge 상태 전환
            if (pc.DodgePressed)
            {
                pc.SetLocomotion(PlayerLocomotionStateId.Dodge);
                return;
            }
            // Move 상태 전환
            if(pc.MoveInput != UnityEngine.Vector2.zero)
            {
                pc.SetLocomotion(PlayerLocomotionStateId.Move);
                return;
            }
        }
        public override void Exit(PlayerFsmContext ctx) { }
    }
}
