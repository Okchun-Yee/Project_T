using UnityEngine;
using ProjectT.Gameplay.Weapon;

namespace ProjectT.Gameplay.Player.FSM
{
    /// <summary>
    /// 플레이어 FSM 컨텍스트 (Step 8: DI 적용)
    /// - Guard/State가 전역 참조 없이 필요한 값에 접근
    /// - IChargingProvider를 통해 차징 값 제공
    /// </summary>
    public sealed class PlayerFsmContext
    {
        public PlayerController Controller { get; }
        public Transform Transform { get; }
        public Rigidbody2D Rigid { get; }
        public Animator Animator { get; }
        
        /// <summary>
        /// 차징 값 제공자 (Guard에서 ctx.ChargingProvider.ChargeNormalized로 접근)
        /// </summary>
        public IChargingProvider ChargingProvider { get; }

        public PlayerFsmContext(PlayerController controller, IChargingProvider chargingProvider = null)
        {
            Controller = controller;
            Transform = controller.transform;
            
            // ChargingProvider: 주입받거나 Singleton fallback
            ChargingProvider = chargingProvider ?? ChargingManager.Instance;

            // 필요 컴포넌트 캐싱 (없으면 null일 수 있음)
            controller.TryGetComponent(out Rigidbody2D rb);
            controller.TryGetComponent(out Animator animator);

            this.Rigid = rb;
            Animator = animator;
        }
    }
}
