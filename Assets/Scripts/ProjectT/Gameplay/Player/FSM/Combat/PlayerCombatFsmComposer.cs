using ProjectT.Core.FSM;
using ProjectT.Gameplay.Player.FSM.Combat.States;

namespace ProjectT.Gameplay.Player.FSM.Combat
{
    /// <summary>
    /// Composer for player combat FSM.
    /// 플레이어 전투 FSM 등록 담당 클래스.
    /// </summary>
    public static class PlayerCombatFsmComposer
    {
        public static StateMachine<PlayerCombatStateId, PlayerFsmContext> Create()
        {
            var fsm = new StateMachine<PlayerCombatStateId, PlayerFsmContext>();

            fsm.RegisterState(PlayerCombatStateId.None,     new CombatNoneState());
            fsm.RegisterState(PlayerCombatStateId.Attack,   new CombatAttackState());
            fsm.RegisterState(PlayerCombatStateId.Charging, new CombatChargingState());
            fsm.RegisterState(PlayerCombatStateId.Holding,  new CombatHoldingState());

            return fsm;
        }
    }
}
