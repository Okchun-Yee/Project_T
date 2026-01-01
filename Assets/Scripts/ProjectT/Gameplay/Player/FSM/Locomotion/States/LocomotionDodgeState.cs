using UnityEngine;

namespace ProjectT.Gameplay.Player.FSM.Locomotion.States
{
    /// <summary>
    /// Locomotion Dodge State
    /// 플레이어 이동 회피 상태
    /// 
    /// Step 5: 타이머 기반 전이는 State 내부에서 처리 (Guard로 중앙화 어려움)
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
            }
        }
        public override void Exit(PlayerFsmContext ctx) { }
    }
}