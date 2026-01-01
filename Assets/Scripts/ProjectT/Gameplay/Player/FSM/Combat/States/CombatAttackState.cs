using System;
using UnityEngine;

namespace ProjectT.Gameplay.Player.FSM.Combat.States
{
    /// <summary>
    /// Combat Attack State
    /// 타이머 기반 전이 (Attack → None)
    /// 
    /// Step 5: 타이머 기반 전이는 State 내부에서 처리
    /// </summary>
    public sealed class CombatAttackState : PlayerCombatStateBase
    {
        private float _timeLeft;
        [SerializeField] private float _attackDuration = 0.5f;
        public float AttackDuration => _attackDuration;

        public override void Enter(PlayerFsmContext ctx)
        {
            _timeLeft = _attackDuration;
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