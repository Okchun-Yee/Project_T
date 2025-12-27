using System;
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

        /// <summary>
        /// 상태 전이 직후 동기적으로 발행되는 콜백
        /// - Update 폴링 대신 이 콜백을 구독하면 누락/중복 없이 전이를 감지할 수 있음
        /// - 콜백 시점: Exit(prev) → Enter(next) → OnStateChanged 발행
        /// </summary>
        public event Action<StateChangedEventArgs<TStateId>> OnStateChanged;

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

            var prevStateId = _currentStateId;

            _currentState.Exit(_context);

            _currentStateId = newStateId;
            _currentState = _states[newStateId];

            _currentState.Enter(_context);

            // 전이 완료 후 즉시 콜백 발행
            OnStateChanged?.Invoke(new StateChangedEventArgs<TStateId>(prevStateId, newStateId));
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
