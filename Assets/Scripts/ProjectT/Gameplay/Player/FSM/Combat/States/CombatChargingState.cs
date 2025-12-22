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
                // 취소 처리 X

                // 공격 상태로 전환
                pc.SetCombat(PlayerCombatStateId.Attack);
                return;
            }
        }
        public override void Exit(PlayerFsmContext ctx) { }
    }
}