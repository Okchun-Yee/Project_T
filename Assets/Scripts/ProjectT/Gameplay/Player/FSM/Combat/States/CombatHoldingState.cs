namespace ProjectT.Gameplay.Player.FSM.Combat.States
{
    /// <summary>
    /// Player combat holding state.
    /// 차징 완료 후 버튼을 누르고 있는 상태
    /// 
    /// 【전이 규칙】
    /// - Holding → Attack: Release (AttackHeld == false, 차징 공격)
    /// - Holding → None: Cancel (Hit/Dodge/Pause/Dead)
    /// 
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
            
            // Release 체크: 버튼을 놓으면 차징 공격으로 전이
            if (!pc.AttackHeld)
            {
                pc.SetCombat(PlayerCombatStateId.Attack);
                return;
            }
            
            // Holding 유지: 버튼을 계속 누르고 있으면 대기
        }
        
        public override void Exit(PlayerFsmContext ctx) { }
    }
}