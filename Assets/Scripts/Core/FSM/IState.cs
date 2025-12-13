namespace ProjectT.core.FSM
{
    public interface IState
    {
        void Enter();
        void Tick();
        void Exit();
    }
}