using System;
using System.Collections.Generic;
using UnityEngine;

namespace _02.Scripts.Player.StateMachine
{
    /// <summary>
    /// 플레이어 상태 머신
    /// 상태 등록 및 전환 관리
    /// </summary>
    public class PlayerStateMachine
    {
        public IPlayerState CurrentState { get; private set; }
        public IPlayerState PreviousState { get; private set; }

        private readonly Dictionary<Type, IPlayerState> _states = new Dictionary<Type, IPlayerState>();

        /// <summary>
        /// 상태 등록
        /// </summary>
        public void RegisterState<T>(T state) where T : IPlayerState
        {
            var type = typeof(T);
            if (_states.ContainsKey(type))
            {
                Debug.LogWarning($"[StateMachine] 상태 중복 등록: {type.Name}");
                return;
            }

            _states[type] = state;
        }

        /// <summary>
        /// 상태 전환
        /// </summary>
        public void ChangeState<T>() where T : IPlayerState
        {
            var type = typeof(T);

            if (!_states.TryGetValue(type, out var newState))
            {
                Debug.LogError($"[StateMachine] 등록되지 않은 상태: {type.Name}");
                return;
            }

            // 같은 상태로 전환 방지
            if (CurrentState?.GetType() == type)
            {
                return;
            }

            PreviousState = CurrentState;
            CurrentState?.Exit();

            CurrentState = newState;
            CurrentState.Enter();
        }

        /// <summary>
        /// 초기 상태 설정
        /// </summary>
        public void Initialize<T>() where T : IPlayerState
        {
            ChangeState<T>();
        }

        /// <summary>
        /// 현재 상태 업데이트
        /// </summary>
        public void Update()
        {
            CurrentState?.Update();
        }

        /// <summary>
        /// 현재 상태 FixedUpdate
        /// </summary>
        public void FixedUpdate()
        {
            CurrentState?.FixedUpdate();
        }

        /// <summary>
        /// 현재 상태가 특정 타입인지 확인
        /// </summary>
        public bool IsCurrentState<T>() where T : IPlayerState
        {
            return CurrentState?.GetType() == typeof(T);
        }

        /// <summary>
        /// 등록된 상태 가져오기
        /// </summary>
        public T GetState<T>() where T : IPlayerState
        {
            if (_states.TryGetValue(typeof(T), out var state))
            {
                return (T)state;
            }

            return default;
        }
    }
}