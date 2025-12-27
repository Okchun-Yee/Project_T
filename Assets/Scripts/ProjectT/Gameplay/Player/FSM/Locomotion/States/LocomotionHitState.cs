using UnityEngine;

namespace ProjectT.Gameplay.Player.FSM.Locomotion.States
{
    /// <summary>
    /// Locomotion Hit State
    /// 플레이어 피격 상태
    /// 
    /// Step 4: State는 전이 결정만, 물리 실행은 Binder에서 처리
    /// </summary>
    public sealed class LocomotionHitState : PlayerLocomotionStateBase
    {
        private float _timeLeft;

        [SerializeField] private float _hitStunDuration = 0.2f;
        public float HitStunDuration => _hitStunDuration;

        public override void Enter(PlayerFsmContext ctx)
        {
            _timeLeft = HitStunDuration;
            // 물리 실행(Stop)은 Binder가 OnStateChanged에서 처리
        }
        public override void Tick(PlayerFsmContext ctx)
        {
            _timeLeft -= Time.deltaTime;

            if(_timeLeft > 0f) return;

            PlayerController pc = ctx.Controller;
            pc.SetLocomotion(pc.MoveInput != Vector2.zero 
                ? PlayerLocomotionStateId.Move 
                : PlayerLocomotionStateId.Idle);
        }
        public override void Exit(PlayerFsmContext ctx) { }
    }
}