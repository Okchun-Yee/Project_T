namespace ProjectT.Core.FSM
{
    public interface IState
    {
        void Enter();
        void Tick();
        void Exit();
    }
}