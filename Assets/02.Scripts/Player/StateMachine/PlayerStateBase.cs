using _02.Scripts.Player.Combat;
using _02.Scripts.Player.Core;
using _02.Scripts.Player.Movement;
using UnityEngine;

namespace _02.Scripts.Player.StateMachine
{
    // 상태 기본 클래스
    public abstract class PlayerStateBase : IPlayerState
    {
        protected readonly PlayerController Controller;
        protected readonly PlayerStateMachine StateMachine;
        protected readonly PlayerInputHandler Input;
        protected readonly PlayerMovement Movement;
        protected readonly DragonSwordSkill Combat;
        protected readonly CharacterController CharacterController;

        protected PlayerStateBase(PlayerController controller, PlayerStateMachine stateMachine)
        {
            Controller = controller;
            StateMachine = stateMachine;
            Input = controller.Input;
            Movement = controller.Movement;
            Combat = controller.DragonSwordSkill;
            CharacterController = controller.CharacterController;
        }

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void Exit() { }

        protected bool HasMoveInput() => Input.MoveInput.sqrMagnitude > 0.01f;
    }
}