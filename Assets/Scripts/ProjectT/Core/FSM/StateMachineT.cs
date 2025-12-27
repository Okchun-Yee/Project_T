using System;
using System.Collections.Generic;

namespace ProjectT.Core.FSM
{
    /// <summary>
    /// 전이 Guard 델리게이트
    /// Context를 받아 전이 가능 여부를 반환
    /// </summary>
    public delegate bool TransitionGuard<TContext>(TContext ctx);

    /// <summary>
    /// 전이 규칙 정의
    /// </summary>
    public readonly struct Transition<TStateId, TContext>
    {
        public readonly TStateId From;
        public readonly TStateId To;
        public readonly TransitionGuard<TContext> Guard;

        public Transition(TStateId from, TStateId to, TransitionGuard<TContext> guard)
        {
            From = from;
            To = to;
            Guard = guard;
        }
    }

    public class StateMachine<TStateId, TContext>
    {
        private readonly Dictionary<TStateId, IState<TContext>> _states = new();
        private readonly List<Transition<TStateId, TContext>> _transitions = new();
        
        private IState<TContext> _currentState;
        private TStateId _currentStateId;

        private TContext _context;

        public TStateId CurrentStateId => _currentStateId;
        public bool IsActive { get; private set; } = true;

        /// <summary>
        /// 상태 전이 직후 동기적으로 발행되는 콜백
        /// </summary>
        public event Action<StateChangedEventArgs<TStateId>> OnStateChanged;

        public void RegisterState(TStateId id, IState<TContext> state)
        {
            _states[id] = state;
        }

        /// <summary>
        /// 전이 규칙 등록
        /// Guard가 true를 반환하면 From → To 전이 수행
        /// </summary>
        public void AddTransition(TStateId from, TStateId to, TransitionGuard<TContext> guard)
        {
            _transitions.Add(new Transition<TStateId, TContext>(from, to, guard));
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
            
            // 1) 등록된 전이 규칙 평가 (먼저 매칭되는 전이 실행)
            foreach (var t in _transitions)
            {
                if (EqualityComparer<TStateId>.Default.Equals(_currentStateId, t.From))
                {
                    if (t.Guard(_context))
                    {
                        ChangeState(t.To);
                        return; // 전이 발생 시 이번 Tick 종료
                    }
                }
            }
            
            // 2) 전이 없으면 현재 상태 Tick 실행
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
