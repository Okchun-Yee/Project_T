using UnityEngine;

namespace ProjectT.Gameplay.Player.FSM.Combat.States
{
    /// <summary>
    /// Player combat charging state.
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