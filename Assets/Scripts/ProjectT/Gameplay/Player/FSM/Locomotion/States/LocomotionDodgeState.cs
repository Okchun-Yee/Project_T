using UnityEngine;

namespace ProjectT.Gameplay.Player.FSM.Locomotion.States
{
    /// <summary>
    /// Locomotion Dodge State
    /// 플레이어 이동 회피 상태
    /// 
    /// Step 4: State는 전이 결정만, 물리 실행은 Binder에서 처리
    /// </summary>
    public sealed class LocomotionDodgeState : PlayerLocomotionStateBase
    {
        private float _timeLeft;
        [SerializeField] private float _dodgeDuration = 0.15f;
        public float DodgeDuration => _dodgeDuration;

        public override void Enter(PlayerFsmContext ctx)
        {
            _timeLeft = DodgeDuration;
        }
        public override void Tick(PlayerFsmContext ctx)
        {
            _timeLeft -= Time.deltaTime;

            if (_timeLeft <= 0f)
            {
                ctx.Controller.SetLocomotion(PlayerLocomotionStateId.Idle);
                return;
            }
        }

        public override void Exit(PlayerFsmContext ctx)
        {
            // 물리 실행(Stop)은 Binder가 OnStateChanged에서 처리
        }
    }
}