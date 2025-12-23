using System;
using UnityEngine;

namespace ProjectT.Gameplay.Player.FSM.Combat.States
{
    /// <summary>
    /// Player combat attack state.
    /// </summary>
    public sealed class CombatAttackState : PlayerCombatStateBase
    {
        private float _timeLeft;
        public override void Enter(PlayerFsmContext ctx)
        {
            ctx.Controller.ChargeTime = 0f;
            ctx.Controller.NotifyAttackStarted();
        }
        public override void Tick(PlayerFsmContext ctx)
        {
            _timeLeft -= Time.deltaTime;
            
            if (_timeLeft > 0f) return;

            ctx.Controller.SetCombat(PlayerCombatStateId.None);
        }
        public override void Exit(PlayerFsmContext ctx)
        {
            ctx.Controller.NotifyAttackEnded();
        }
    }
} 