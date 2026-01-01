using UnityEngine;

namespace ProjectT.Gameplay.Player.FSM.Locomotion.States
{
    /// <summary>
    /// Locomotion Hit State
    /// 플레이어 피격 상태
    /// 
    /// Step 5: 타이머 기반 전이는 State 내부에서 처리 (Guard로 중앙화 어려움)
    /// </summary>
    public sealed class LocomotionHitState : PlayerLocomotionStateBase
    {
        private float _timeLeft;
        [SerializeField] private float _hitStunDuration = 0.2f;
        public float HitStunDuration => _hitStunDuration;

        public override void Enter(PlayerFsmContext ctx)
        {
            _timeLeft = HitStunDuration;
        }
        public override void Tick(PlayerFsmContext ctx)
        {
            _timeLeft -= Time.deltaTime;
            if (_timeLeft > 0f) return;

            PlayerController pc = ctx.Controller;
            pc.SetLocomotion(pc.MoveInput != Vector2.zero
                ? PlayerLocomotionStateId.Move
                : PlayerLocomotionStateId.Idle);
        }
        public override void Exit(PlayerFsmContext ctx) { }
    }
}