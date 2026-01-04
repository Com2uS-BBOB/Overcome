using _02.Scripts.Player.Combat;
using _02.Scripts.Player.Core;
using _02.Scripts.Player.Movement;
using UnityEngine;

namespace _02.Scripts.Player.StateMachine
{
    /// <summary>
    /// 플레이어 상태 기본 클래스
    /// 공통 참조 및 유틸리티 제공
    /// </summary>
    public abstract class PlayerStateBase : IPlayerState
    {
        protected readonly PlayerController Controller;
        protected readonly PlayerStateMachine StateMachine;
        protected readonly PlayerInputHandler Input;
        protected readonly PlayerMovement Movement;
        protected readonly PlayerCombat Combat;
        protected readonly CharacterController CharacterController;

        protected PlayerStateBase(PlayerController controller, PlayerStateMachine stateMachine)
        {
            Controller = controller;
            StateMachine = stateMachine;
            Input = controller.Input;
            Movement = controller.Movement;
            Combat = controller.Combat;
            CharacterController = controller.CharacterController;
        }

        public virtual void Enter()
        {
            Debug.Log($"[State] Enter: {GetType().Name}");
        }

        public virtual void Update()
        {
        }

        public virtual void FixedUpdate()
        {
        }

        public virtual void Exit()
        {
            Debug.Log($"[State] Exit: {GetType().Name}");
        }

        /// <summary>
        /// 이동 입력 있는지 확인
        /// </summary>
        protected bool HasMoveInput()
        {
            return Input.MoveInput.sqrMagnitude > 0.01f;
        }
    }
}