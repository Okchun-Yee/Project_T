namespace ProjectT.Core.FSM
{
    public interface IState<TContext>
    {
        void Enter(TContext context);
        void Tick(TContext context);
        void Exit(TContext context);
    }
}