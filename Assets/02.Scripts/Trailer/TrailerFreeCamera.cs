using UnityEngine;
using UnityEngine.InputSystem;

namespace _02.Scripts.Trailer
{
    /// <summary>
    /// 트레일러용 자유 카메라
    /// WASD 이동 + 마우스 회전으로 직접 조종
    /// </summary>
    public class TrailerFreeCamera : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _fastSpeed = 15f;
        [SerializeField] private float _smoothTime = 0.1f;

        [Header("Rotation")]
        [SerializeField] private float _lookSensitivity = 0.1f;
        [SerializeField] private bool _invertY = false;

        [Header("Options")]
        [SerializeField] private bool _enableOnStart = true;
        [SerializeField] private bool _lockCursor = true;

        private Vector3 _velocity;
        private Vector3 _targetPosition;
        private float _rotationX;
        private float _rotationY;
        private bool _isEnabled;

        private Keyboard _keyboard;
        private Mouse _mouse;

        private void Start()
        {
            _keyboard = Keyboard.current;
            _mouse = Mouse.current;

            _targetPosition = transform.position;
            _rotationX = transform.eulerAngles.y;
            _rotationY = transform.eulerAngles.x;

            if (_enableOnStart)
                Enable();
        }

        private void Update()
        {
            if (!_isEnabled) return;
            if (_keyboard == null || _mouse == null) return;

            HandleRotation();
            HandleMovement();

            // ESC로 커서 해제
            if (_keyboard.escapeKey.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            // 클릭으로 다시 잠금
            if (_mouse.leftButton.wasPressedThisFrame && _lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void HandleRotation()
        {
            // 마우스 입력
            Vector2 mouseDelta = _mouse.delta.ReadValue() * _lookSensitivity;

            _rotationX += mouseDelta.x;
            _rotationY += _invertY ? mouseDelta.y : -mouseDelta.y;
            _rotationY = Mathf.Clamp(_rotationY, -90f, 90f);

            transform.rotation = Quaternion.Euler(_rotationY, _rotationX, 0f);
        }

        private void HandleMovement()
        {
            // WASD + QE 입력
            float horizontal = 0f;
            float vertical = 0f;
            float upDown = 0f;

            if (_keyboard.aKey.isPressed) horizontal -= 1f;
            if (_keyboard.dKey.isPressed) horizontal += 1f;
            if (_keyboard.wKey.isPressed) vertical += 1f;
            if (_keyboard.sKey.isPressed) vertical -= 1f;
            if (_keyboard.eKey.isPressed) upDown += 1f;
            if (_keyboard.qKey.isPressed) upDown -= 1f;

            // 이동 방향
            Vector3 direction = transform.right * horizontal
                              + transform.forward * vertical
                              + Vector3.up * upDown;

            // Shift로 빠른 이동
            float speed = _keyboard.leftShiftKey.isPressed ? _fastSpeed : _moveSpeed;

            _targetPosition += direction.normalized * speed * Time.deltaTime;

            // 부드러운 이동
            transform.position = Vector3.SmoothDamp(transform.position, _targetPosition, ref _velocity, _smoothTime);
        }

        [ContextMenu("Enable Camera")]
        public void Enable()
        {
            _isEnabled = true;
            _targetPosition = transform.position;

            if (_lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            Debug.Log("[TrailerFreeCamera] Enabled");
        }

        [ContextMenu("Disable Camera")]
        public void Disable()
        {
            _isEnabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Debug.Log("[TrailerFreeCamera] Disabled");
        }
    }
}