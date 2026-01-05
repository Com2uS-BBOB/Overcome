using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _02.Scripts.Player.Core
{
    // 입력 처리기 (InputActionAsset 사용)
    public class PlayerInputHandler : MonoBehaviour
    {
        [Header("Input Actions Asset")]
        [SerializeField] private InputActionAsset _inputActions;

        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _attackAction;
        private InputAction _sprintAction;
        private InputAction _jumpAction;
        private InputAction _crescentAction;
        private InputAction _overDriveAction;

        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }

        public event Action OnAttackStarted;
        public event Action OnAttackPerformed;
        public event Action OnDashAttackPerformed;
        public event Action OnJumpPerformed;
        public event Action OnCrescentPerformed;
        public event Action OnOverDrivePerformed;

        private void Awake() => SetupActions();
        private void OnEnable() { EnableActions(); SubscribeEvents(); }
        private void OnDisable() { UnsubscribeEvents(); DisableActions(); }

        private void SetupActions()
        {
            if (_inputActions == null) return;

            var playerMap = _inputActions.FindActionMap("Player");
            if (playerMap == null) return;

            _moveAction = playerMap.FindAction("Move");
            _lookAction = playerMap.FindAction("Look");
            _attackAction = playerMap.FindAction("Attack");
            _sprintAction = playerMap.FindAction("Sprint");
            _jumpAction = playerMap.FindAction("Jump");
            _crescentAction = playerMap.FindAction("Crescent");
            _overDriveAction = playerMap.FindAction("OverDrive");
        }

        private void EnableActions()
        {
            _moveAction?.Enable();
            _lookAction?.Enable();
            _attackAction?.Enable();
            _sprintAction?.Enable();
            _jumpAction?.Enable();
            _crescentAction?.Enable();
            _overDriveAction?.Enable();
        }

        private void DisableActions()
        {
            _moveAction?.Disable();
            _lookAction?.Disable();
            _attackAction?.Disable();
            _sprintAction?.Disable();
            _jumpAction?.Disable();
            _crescentAction?.Disable();
            _overDriveAction?.Disable();
        }

        private void SubscribeEvents()
        {
            if (_moveAction != null) { _moveAction.performed += ctx => MoveInput = ctx.ReadValue<Vector2>(); _moveAction.canceled += _ => MoveInput = Vector2.zero; }
            if (_lookAction != null) { _lookAction.performed += ctx => LookInput = ctx.ReadValue<Vector2>(); _lookAction.canceled += _ => LookInput = Vector2.zero; }
            if (_attackAction != null) { _attackAction.started += _ => OnAttackStarted?.Invoke(); _attackAction.performed += _ => OnAttackPerformed?.Invoke(); }
            if (_sprintAction != null) _sprintAction.performed += _ => OnDashAttackPerformed?.Invoke();
            if (_jumpAction != null) _jumpAction.performed += _ => OnJumpPerformed?.Invoke();
            if (_crescentAction != null) _crescentAction.performed += _ => OnCrescentPerformed?.Invoke();
            if (_overDriveAction != null) _overDriveAction.performed += _ => OnOverDrivePerformed?.Invoke();
        }

        private void UnsubscribeEvents()
        {
            _moveAction?.Disable();
            _lookAction?.Disable();
            _attackAction?.Disable();
            _sprintAction?.Disable();
            _jumpAction?.Disable();
            _crescentAction?.Disable();
            _overDriveAction?.Disable();
        }
    }
}