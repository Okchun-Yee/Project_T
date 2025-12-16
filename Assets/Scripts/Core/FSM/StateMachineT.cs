using System.Collections.Generic;

namespace ProjectT.Core.FSM
{
    public class StateMachine<TStateId, TContext>
    {
        private readonly Dictionary<TStateId, IState<TContext>> _states = new();
        
        private IState<TContext> _currentState;
        private TStateId _currentStateId;

        private TContext _context;

        public TStateId CurrentStateId => _currentStateId;
        public bool IsActive { get; private set; } = true;

        public void RegisterState(TStateId id, IState<TContext> state)
        {
            _states[id] = state;
        }

        public void Initialize(TContext context, TStateId startStateId)
        {
            _context = context;

            _currentStateId = startStateId;
            _currentState = _states[startStateId];
            _currentState.Enter(_context);
        }

        public void ChangeState(TStateId newStateId)
        {
            if (!IsActive) return;
            if (EqualityComparer<TStateId>.Default.Equals(_currentStateId, newStateId))
                return;

            _currentState.Exit(_context);

            _currentStateId = newStateId;
            _currentState = _states[newStateId];

            _currentState.Enter(_context);
        }

        public void Tick()
        {
            if (!IsActive) return;
            _currentState?.Tick(_context);
        }

        public void Deactivate()
        {
            if (!IsActive) return;

            IsActive = false;
            _currentState?.Exit(_context);
            _currentState = null;
        }
    }
}
