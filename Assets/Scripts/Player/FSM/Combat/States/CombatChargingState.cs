using UnityEngine;

namespace ProjectT.Game.Player.FSM.Combat.States
{
    /// <summary>
    /// Player combat charging state.
    /// </summary>
    public sealed class CombatChargingState : PlayerCombatStateBase
    {
        public override void Enter(PlayerFsmContext ctx)
        {
            ctx.Controller.ChargeTime = 0f;
        }
        public override void Tick(PlayerFsmContext ctx)
        {
            PlayerController pc = ctx.Controller;
            if(!pc.AttackHeld)
            {
                pc.SetCombat(PlayerCombatStateId.None);
                return;
            }
            pc.ChargeTime += Time.deltaTime;

            if(pc.ChargeTime >= pc.MaxChargeTime)
            {
                pc.SetCombat(PlayerCombatStateId.Holding);
            }
        }
        public override void Exit(PlayerFsmContext ctx) { }
    }
}