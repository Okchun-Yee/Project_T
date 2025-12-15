using UnityEngine;

namespace ProjectT.Game.Player.FSM
{
    /// <summary>
    /// * 플레이어 FSM 컨텍스트
    /// * sealed class로 선언하여 상속 금지
    /// 플레이어 상태들이 참조할 데이터를 담고 있음
    /// 필요한 컴포넌트들을 미리 캐싱해두어 상태들이 쉽게 접근할 수 있도록 함
    /// 필요할 경우 여기에 더 많은 데이터를 추가할 수 있음
    /// </summary>
    public sealed class PlayerFsmContext
    {
        public PlayerController Controller { get; }
        public Transform Transform { get; }
        public Rigidbody2D rb { get; }
        public Animator Animator { get; }

        public PlayerFsmContext(PlayerController controller)
        {
            Controller = controller;
            Transform = controller.transform;

            // 필요 컴포넌트 캐싱 (없으면 null일 수 있음)
            controller.TryGetComponent(out Rigidbody2D rb);
            controller.TryGetComponent(out Animator animator);

            this.rb = rb;
            Animator = animator;
        }
    }
}
