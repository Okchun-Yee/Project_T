using System;
using UnityEngine;

namespace ProjectT.Gameplay.Player.FSM.Combat.States
{
    /// <summary>
    /// Combat Attack State
    /// 타이머 기반 전이 (Attack → None)
    /// 
    /// Step 5: 타이머 기반 전이는 State 내부에서 처리
    /// WeaponSO의 weaponCooldown을 Attack 상태 지속시간으로 사용
    /// </summary>
    public sealed class CombatAttackState : PlayerCombatStateBase
    {
        private float _timeLeft;
        [SerializeField] private float _attackDuration = 0.2f;  // fallback 기본값
        public float AttackDuration => _attackDuration;

        public override void Enter(PlayerFsmContext ctx)
        {
            // WeaponSO의 쿨타임을 Attack 상태 지속시간으로 사용
            if (ActiveWeapon.Instance?.currentWeapon != null)
            {
                _timeLeft = ActiveWeapon.Instance.currentWeapon.GetWeaponInfo().weaponCooldown;
            }
            else
            {
                _timeLeft = _attackDuration;  // 무기가 없으면 기본값 사용
            }
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