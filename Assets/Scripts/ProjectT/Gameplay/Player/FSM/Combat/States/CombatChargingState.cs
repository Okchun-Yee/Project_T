using ProjectT.Gameplay.Weapon;
using UnityEngine;

namespace ProjectT.Gameplay.Player.FSM.Combat.States
{
    /// <summary>
    /// Player combat charging state.
    /// 
    /// 【전이 규칙】
    /// - Charging → Holding: ChargeNormalized >= 1 (딱 1회, 값 기반)
    /// - Charging → Attack: Release (AttackHeld == false, 차징 미완료)
    /// - Charging → None: Cancel (Hit/Dodge/Pause/Dead)
    /// 
    /// 이벤트 발행은 Binder가 담당 (State는 결정만)
    /// </summary>
    public sealed class CombatChargingState : PlayerCombatStateBase
    {
        public override void Enter(PlayerFsmContext ctx)
        {
            // 이벤트 발행은 Binder가 전이 감지로 처리
        }
        
        public override void Tick(PlayerFsmContext ctx)
        {
            PlayerController pc = ctx.Controller;
            
            // 1) Release 체크: 버튼을 놓으면 일반 공격으로 전이
            if (!pc.AttackHeld)
            {
                pc.SetCombat(PlayerCombatStateId.Attack);
                return;
            }
            
            // 2) Max 도달 체크: ChargeNormalized >= 1 이면 Holding으로 전이
            //    ChargingManager의 값을 기준으로 판단 (SSOT)
            var cm = ChargingManager.Instance;
            if (cm != null && cm.ChargeNormalized >= 1f)
            {
                pc.SetCombat(PlayerCombatStateId.Holding);
                return;
            }
        }
        
        public override void Exit(PlayerFsmContext ctx) { }
    }
}