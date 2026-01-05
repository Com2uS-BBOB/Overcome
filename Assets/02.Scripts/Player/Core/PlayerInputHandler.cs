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
            if (_moveAction != null) { _moveAction.performed += OnMovePerformed; _moveAction.canceled += OnMoveCanceled; }
            if (_lookAction != null) { _lookAction.performed += OnLookPerformed; _lookAction.canceled += OnLookCanceled; }
            if (_attackAction != null) { _attackAction.started += OnAttackActionStarted; _attackAction.performed += OnAttackActionPerformed; }
            if (_sprintAction != null) _sprintAction.performed += OnSprintPerformed;
            if (_jumpAction != null) _jumpAction.performed += OnJumpActionPerformed;
            if (_crescentAction != null) _crescentAction.performed += OnCrescentActionPerformed;
            if (_overDriveAction != null) _overDriveAction.performed += OnOverDriveActionPerformed;
        }

        private void UnsubscribeEvents()
        {
            if (_moveAction != null) { _moveAction.performed -= OnMovePerformed; _moveAction.canceled -= OnMoveCanceled; }
            if (_lookAction != null) { _lookAction.performed -= OnLookPerformed; _lookAction.canceled -= OnLookCanceled; }
            if (_attackAction != null) { _attackAction.started -= OnAttackActionStarted; _attackAction.performed -= OnAttackActionPerformed; }
            if (_sprintAction != null) _sprintAction.performed -= OnSprintPerformed;
            if (_jumpAction != null) _jumpAction.performed -= OnJumpActionPerformed;
            if (_crescentAction != null) _crescentAction.performed -= OnCrescentActionPerformed;
            if (_overDriveAction != null) _overDriveAction.performed -= OnOverDriveActionPerformed;
        }

        #region Event Handlers
        private void OnMovePerformed(InputAction.CallbackContext ctx) => MoveInput = ctx.ReadValue<Vector2>();
        private void OnMoveCanceled(InputAction.CallbackContext _) => MoveInput = Vector2.zero;
        private void OnLookPerformed(InputAction.CallbackContext ctx) => LookInput = ctx.ReadValue<Vector2>();
        private void OnLookCanceled(InputAction.CallbackContext _) => LookInput = Vector2.zero;
        private void OnAttackActionStarted(InputAction.CallbackContext _) => OnAttackStarted?.Invoke();
        private void OnAttackActionPerformed(InputAction.CallbackContext _) => OnAttackPerformed?.Invoke();
        private void OnSprintPerformed(InputAction.CallbackContext _) => OnDashAttackPerformed?.Invoke();
        private void OnJumpActionPerformed(InputAction.CallbackContext _) => OnJumpPerformed?.Invoke();
        private void OnCrescentActionPerformed(InputAction.CallbackContext _) => OnCrescentPerformed?.Invoke();
        private void OnOverDriveActionPerformed(InputAction.CallbackContext _) => OnOverDrivePerformed?.Invoke();
        #endregion
    }
}