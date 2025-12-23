using ProjectT.Core.FSM;

namespace ProjectT.Gameplay.Player.FSM
{
    /// <summary>
    /// * 플레이어 상태들의 공통 베이스 클래스
    /// Player 전용 상태 명시
    /// 공통 접근 경로 제공
    /// 공통 정책을 외부에서 강제할 수 있음
    /// 확장 포인트 제공 (Hooks)
    /// </summary>
    public abstract class PlayerStateBase : IState<PlayerFsmContext>
    {
        public virtual void Enter(PlayerFsmContext ctx) { }
        public virtual void Exit(PlayerFsmContext ctx) { }
        public virtual void Tick(PlayerFsmContext ctx) { }

        // 자주 쓰는 단축 접근자
        protected PlayerController PC(PlayerFsmContext ctx) => ctx.Controller;
    }
}
