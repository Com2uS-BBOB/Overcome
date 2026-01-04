using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _02.Scripts.Player.Core
{
    /// <summary>
    /// C# Events 방식 입력 처리기
    /// InputActionAsset 직접 참조
    /// </summary>
    public class PlayerInputHandler : MonoBehaviour
    {
        [Header("Input Actions Asset")]
        [SerializeField] private InputActionAsset _inputActions;

        // Input Actions
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _attackAction;
        private InputAction _sprintAction;
        private InputAction _jumpAction;

        // 입력 값
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }

        // 입력 이벤트
        public event Action OnAttackStarted;
        public event Action OnAttackPerformed;
        public event Action OnDashAttackPerformed;
        public event Action OnJumpPerformed;

        private void Awake()
        {
            SetupActions();
        }

        private void OnEnable()
        {
            EnableActions();
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
            DisableActions();
        }

        private void SetupActions()
        {
            if (_inputActions == null)
            {
                Debug.LogError("[PlayerInputHandler] InputActionAsset이 할당되지 않았습니다!");
                return;
            }

            var playerMap = _inputActions.FindActionMap("Player");
            if (playerMap == null)
            {
                Debug.LogError("[PlayerInputHandler] 'Player' ActionMap을 찾을 수 없습니다!");
                return;
            }

            _moveAction = playerMap.FindAction("Move");
            _lookAction = playerMap.FindAction("Look");
            _attackAction = playerMap.FindAction("Attack");
            _sprintAction = playerMap.FindAction("Sprint");
            _jumpAction = playerMap.FindAction("Jump");
        }

        private void EnableActions()
        {
            _moveAction?.Enable();
            _lookAction?.Enable();
            _attackAction?.Enable();
            _sprintAction?.Enable();
            _jumpAction?.Enable();
        }

        private void DisableActions()
        {
            _moveAction?.Disable();
            _lookAction?.Disable();
            _attackAction?.Disable();
            _sprintAction?.Disable();
            _jumpAction?.Disable();
        }

        private void SubscribeEvents()
        {
            if (_moveAction != null)
            {
                _moveAction.performed += OnMovePerformed;
                _moveAction.canceled += OnMoveCanceled;
            }

            if (_lookAction != null)
            {
                _lookAction.performed += OnLookPerformed;
                _lookAction.canceled += OnLookCanceled;
            }

            if (_attackAction != null)
            {
                _attackAction.started += OnAttackStartedCallback;
                _attackAction.performed += OnAttackPerformedCallback;
            }

            if (_sprintAction != null)
            {
                _sprintAction.performed += OnSprintPerformed;
            }

            if (_jumpAction != null)
            {
                _jumpAction.performed += OnJumpPerformedCallback;
            }
        }

        private void UnsubscribeEvents()
        {
            if (_moveAction != null)
            {
                _moveAction.performed -= OnMovePerformed;
                _moveAction.canceled -= OnMoveCanceled;
            }

            if (_lookAction != null)
            {
                _lookAction.performed -= OnLookPerformed;
                _lookAction.canceled -= OnLookCanceled;
            }

            if (_attackAction != null)
            {
                _attackAction.started -= OnAttackStartedCallback;
                _attackAction.performed -= OnAttackPerformedCallback;
            }

            if (_sprintAction != null)
            {
                _sprintAction.performed -= OnSprintPerformed;
            }

            if (_jumpAction != null)
            {
                _jumpAction.performed -= OnJumpPerformedCallback;
            }
        }

        #region Input Callbacks

        private void OnMovePerformed(InputAction.CallbackContext ctx)
        {
            MoveInput = ctx.ReadValue<Vector2>();
        }

        private void OnMoveCanceled(InputAction.CallbackContext ctx)
        {
            MoveInput = Vector2.zero;
        }

        private void OnLookPerformed(InputAction.CallbackContext ctx)
        {
            LookInput = ctx.ReadValue<Vector2>();
        }

        private void OnLookCanceled(InputAction.CallbackContext ctx)
        {
            LookInput = Vector2.zero;
        }

        private void OnAttackStartedCallback(InputAction.CallbackContext ctx)
        {
            OnAttackStarted?.Invoke();
        }

        private void OnAttackPerformedCallback(InputAction.CallbackContext ctx)
        {
            Debug.Log("[Input] Attack (용검)");
            OnAttackPerformed?.Invoke();
        }

        private void OnSprintPerformed(InputAction.CallbackContext ctx)
        {
            Debug.Log("[Input] DashAttack (질풍참)");
            OnDashAttackPerformed?.Invoke();
        }

        private void OnJumpPerformedCallback(InputAction.CallbackContext ctx)
        {
            Debug.Log("[Input] Jump");
            OnJumpPerformed?.Invoke();
        }

        #endregion
    }
}