using System.Collections.Generic;

namespace ProjectT.Core.FSM
{
    public class StateMachine<TStateId>
    {
        private readonly Dictionary<TStateId, IState> _states = new Dictionary<TStateId, IState>();

        private IState _currentState;
        private TStateId _currentStateId;

        // properties
        public TStateId CurrentStateId => _currentStateId;
        public bool IsActive { get; private set; } = true;

        // 상태 등록
        public void RegisterState(TStateId id, IState state)
        {
            _states[id] = state;
        }
        // 상태 초기화
        // 초기 상태 지정
        public void Initialize(TStateId startStateId)
        {
            _currentStateId = startStateId;
            _currentState = _states[startStateId];
            _currentState.Enter();
        }

        // 상태 전이
        public void ChangeState(TStateId newStateId)
        {
            if (!IsActive) return;
            if (EqualityComparer<TStateId>.Default.Equals(_currentStateId, newStateId))
                return;

            _currentState.Exit();

            _currentStateId = newStateId;
            _currentState = _states[newStateId];

            _currentState.Enter();
        }

        // 매 프레임 호출
        public void Tick()
        {
            if (!IsActive) return;
            _currentState?.Tick();
        }

        // 강제 비활성 (Dead 등)
        public void Deactivate()
        {
            if (!IsActive) return;

            IsActive = false;
            _currentState?.Exit();
            _currentState = null;
        }
    }
}