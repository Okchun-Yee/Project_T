using System;
using UnityEngine;

namespace ProjectT.Gameplay.Player.FSM.Combat.States
{
    /// <summary>
    /// Player combat attack state.
    /// 이벤트 발행은 Binder가 담당 (State는 결정만)
    /// </summary>
    public sealed class CombatAttackState : PlayerCombatStateBase
    {
        private float _timeLeft;
        public override void Enter(PlayerFsmContext ctx)
        {
            // 이벤트 발행은 Binder가 전이 감지로 처리
        }
        public override void Tick(PlayerFsmContext ctx)
        {
            _timeLeft -= Time.deltaTime;
            
            if (_timeLeft > 0f) return;

            ctx.Controller.SetCombat(PlayerCombatStateId.None);
        }
        public override void Exit(PlayerFsmContext ctx)
        {
            // 이벤트 발행은 Binder가 전이 감지로 처리
        }
    }
} 