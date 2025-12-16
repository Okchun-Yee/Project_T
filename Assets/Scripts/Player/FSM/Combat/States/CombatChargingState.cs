using UnityEngine;

namespace ProjectT.Game.Player.FSM.Combat.States
{
    /// <summary>
    /// Player combat charging state.
    /// </summary>
    public sealed class CombatChargingState : PlayerCombatStateBase
    {
        private bool _reachedMax;
        public override void Enter(PlayerFsmContext ctx)
        {
            _reachedMax = false;
            ctx.Controller.ChargeTime = 0f;
            ctx.Controller.NotifyChargeStarted();
        }
        public override void Tick(PlayerFsmContext ctx)
        {
            PlayerController pc = ctx.Controller;
            if(!pc.AttackHeld)
            {
                pc.NotifyChargeCanceled();
                pc.SetCombat(PlayerCombatStateId.None);
                return;
            }
            pc.ChargeTime += Time.deltaTime;

            if(!_reachedMax && pc.ChargeTime >= pc.MaxChargeTime)
            {
                _reachedMax = true;
                pc.NotifyChargeReachedMax();
                pc.SetCombat(PlayerCombatStateId.Holding);
                return;
            }
        }
        public override void Exit(PlayerFsmContext ctx) { }
    }
}