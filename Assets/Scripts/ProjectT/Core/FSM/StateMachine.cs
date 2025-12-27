using System;
using System.Collections.Generic;

namespace ProjectT.Core.FSM
{
    /// <summary>
    /// 상태 전이 발생 시 즉시 발행되는 콜백 이벤트 인자
    /// </summary>
    public readonly struct StateChangedEventArgs<TStateId>
    {
        public readonly TStateId PrevStateId;
        public readonly TStateId NextStateId;

        public StateChangedEventArgs(TStateId prev, TStateId next)
        {
            PrevStateId = prev;
            NextStateId = next;
        }
    }

    public class StateMachine<TStateId>
    {
        private readonly Dictionary<TStateId, IState> _states = new Dictionary<TStateId, IState>();

        private IState _currentState;
        private TStateId _currentStateId;

        // properties
        public TStateId CurrentStateId => _currentStateId;
        public bool IsActive { get; private set; } = true;

        /// <summary>
        /// 상태 전이 직후 동기적으로 발행되는 콜백
        /// - Update 폴링 대신 이 콜백을 구독하면 누락/중복 없이 전이를 감지할 수 있음
        /// - 콜백 시점: Exit(prev) → Enter(next) → OnStateChanged 발행
        /// </summary>
        public event Action<StateChangedEventArgs<TStateId>> OnStateChanged;

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

            var prevStateId = _currentStateId;

            _currentState.Exit();

            _currentStateId = newStateId;
            _currentState = _states[newStateId];

            _currentState.Enter();

            // 전이 완료 후 즉시 콜백 발행
            OnStateChanged?.Invoke(new StateChangedEventArgs<TStateId>(prevStateId, newStateId));
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