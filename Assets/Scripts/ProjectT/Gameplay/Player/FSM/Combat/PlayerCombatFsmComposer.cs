using ProjectT.Core.FSM;
using ProjectT.Gameplay.Player.FSM.Combat.States;

namespace ProjectT.Gameplay.Player.FSM.Combat
{
    /// <summary>
    /// Combat FSM Composer
    /// - 상태 등록
    /// - 전이 규칙 등록 (Guard 기반)
    /// 
    /// Step 8: Guard가 ctx.ChargingProvider로 접근 (전역 참조 제거)
    /// </summary>
    public static class PlayerCombatFsmComposer
    {
        public static StateMachine<PlayerCombatStateId, PlayerFsmContext> Create()
        {
            var fsm = new StateMachine<PlayerCombatStateId, PlayerFsmContext>();

            // 상태 등록
            fsm.RegisterState(PlayerCombatStateId.None,     new CombatNoneState());
            fsm.RegisterState(PlayerCombatStateId.Attack,   new CombatAttackState());
            fsm.RegisterState(PlayerCombatStateId.Charging, new CombatChargingState());
            fsm.RegisterState(PlayerCombatStateId.Holding,  new CombatHoldingState());

            // 전이 규칙 등록
            RegisterTransitions(fsm);

            return fsm;
        }

        /// <summary>
        /// Combat 전이 규칙 (우선순위 순서로 등록)
        /// </summary>
        private static void RegisterTransitions(StateMachine<PlayerCombatStateId, PlayerFsmContext> fsm)
        {
            // None → Charging: AttackPressed && CanCharge
            fsm.AddTransition(PlayerCombatStateId.None, PlayerCombatStateId.Charging,
                ctx => ctx.Controller.AttackPressed && ctx.Controller.CanChargeAttack);

            // None → Attack: AttackPressed && !CanCharge
            fsm.AddTransition(PlayerCombatStateId.None, PlayerCombatStateId.Attack,
                ctx => ctx.Controller.AttackPressed && !ctx.Controller.CanChargeAttack);

            // Charging → Holding: ChargeNormalized >= 1 (Step 8: ctx 기반)
            fsm.AddTransition(PlayerCombatStateId.Charging, PlayerCombatStateId.Holding,
                ctx => ctx.ChargingProvider != null && ctx.ChargingProvider.ChargeNormalized >= 1f);

            // Charging → Attack: Release (AttackHeld == false) OR quick click
            fsm.AddTransition(PlayerCombatStateId.Charging, PlayerCombatStateId.Attack,
                ctx => !ctx.Controller.AttackHeld || ctx.Controller.AttackPressed);

            // Holding → Attack: Release (AttackHeld == false)
            fsm.AddTransition(PlayerCombatStateId.Holding, PlayerCombatStateId.Attack,
                ctx => !ctx.Controller.AttackHeld);

            // Attack → None: 타이머 종료 (State 내부에서 처리, 여기선 등록 안함)
            // Cancel(Hit/Dodge/Pause/Dead) → None: 외부 강제 전이 (Step7)
        }
    }
}
