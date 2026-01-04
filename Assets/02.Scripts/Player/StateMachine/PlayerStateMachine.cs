using System;
using System.Collections.Generic;

namespace _02.Scripts.Player.StateMachine
{
    // 상태 머신
    public class PlayerStateMachine
    {
        public IPlayerState CurrentState { get; private set; }
        public IPlayerState PreviousState { get; private set; }

        private readonly Dictionary<Type, IPlayerState> _states = new();

        public void RegisterState<T>(T state) where T : IPlayerState
        {
            var type = typeof(T);
            if (!_states.ContainsKey(type)) _states[type] = state;
        }

        public void ChangeState<T>() where T : IPlayerState
        {
            var type = typeof(T);
            if (!_states.TryGetValue(type, out var newState)) return;
            if (CurrentState?.GetType() == type) return;

            PreviousState = CurrentState;
            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState.Enter();
        }

        public void Initialize<T>() where T : IPlayerState => ChangeState<T>();
        public void Update() => CurrentState?.Update();
        public void FixedUpdate() => CurrentState?.FixedUpdate();
        public bool IsCurrentState<T>() where T : IPlayerState => CurrentState?.GetType() == typeof(T);

        public T GetState<T>() where T : IPlayerState
        {
            return _states.TryGetValue(typeof(T), out var state) ? (T)state : default;
        }
    }
}