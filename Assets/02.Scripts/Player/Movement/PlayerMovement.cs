using System;
using System.Collections;
using UnityEngine;
using _02.Scripts.Common;
using _02.Scripts.Player.Data;

namespace _02.Scripts.Player.Movement
{
    // 플레이어 이동 (CharacterController 기반)
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        private const string DashCooldownKey = "Dash";

        [Header("Settings")]
        [SerializeField] private float _rotationSpeed = 10f;
        [SerializeField] private float _dashDuration = 0.3f;

        [Header("Gravity")]
        [SerializeField] private float _gravity = -20f;
        [SerializeField] private float _groundCheckDistance = 0.2f;
        [SerializeField] private LayerMask _groundLayer;

        [Header("References")]
        [SerializeField] private Transform _cameraTransform;

        private CharacterController _controller;
        private PlayerRuntimeStats _stats;
        private CooldownManager _cooldownManager;
        private Vector3 _velocity;
        private bool _isGrounded;
        private bool _isDashing;

        private float MoveSpeed => _stats != null ? _stats.MoveSpeed : 8f;
        private float JumpForce => _stats != null ? _stats.JumpForce : 10f;
        private float DashDistance => _stats != null ? _stats.DashDistance : 10f;
        private float DashCooldown => _stats != null ? _stats.DashCooldown : 3f;

        public bool IsGrounded => _isGrounded;
        public bool IsMoving { get; private set; }
        public bool IsDashing => _isDashing;
        public bool CanDash => !_isDashing && _cooldownManager.IsReady(DashCooldownKey, DashCooldown);

        public event Action OnDashStarted;
        public event Action OnDashEnded;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _stats = GetComponent<PlayerRuntimeStats>();
            _cooldownManager = new CooldownManager();

            if (_cameraTransform == null) _cameraTransform = Camera.main?.transform;
        }

        public void Initialize(PlayerRuntimeStats stats) => _stats = stats;

        private void Update()
        {
            CheckGround();
            ApplyGravity();
            RotateToCamera();
        }

        // 카메라 방향으로 회전
        private void RotateToCamera()
        {
            if (_cameraTransform == null) return;

            Vector3 forward = _cameraTransform.forward;
            forward.y = 0f;
            forward.Normalize();

            if (forward.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(forward);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            }
        }

        // 카메라 기준 이동
        public void Move(Vector2 input)
        {
            if (input.sqrMagnitude < 0.01f)
            {
                IsMoving = false;
                return;
            }

            IsMoving = true;

            Vector3 forward = _cameraTransform.forward;
            Vector3 right = _cameraTransform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 moveDirection = forward * input.y + right * input.x;
            _controller.Move(moveDirection * MoveSpeed * Time.deltaTime);
        }

        public void Jump()
        {
            if (!_isGrounded && !_controller.isGrounded) return;
            _velocity.y = JumpForce;
        }

        private void CheckGround()
        {
            _isGrounded = _controller.isGrounded;

            if (!_isGrounded)
            {
                _isGrounded = Physics.SphereCast(
                    transform.position + Vector3.up * (_controller.radius + 0.1f),
                    _controller.radius * 0.9f, Vector3.down, out _,
                    _groundCheckDistance + 0.1f, _groundLayer);
            }

            if (!_isGrounded && _groundLayer == 0)
            {
                _isGrounded = Physics.Raycast(
                    transform.position + Vector3.up * 0.1f,
                    Vector3.down, _groundCheckDistance + 0.2f);
            }

            if (_isGrounded && _velocity.y < 0) _velocity.y = -2f;
        }

        private void ApplyGravity()
        {
            _velocity.y += _gravity * Time.deltaTime;
            _controller.Move(_velocity * Time.deltaTime);
        }

        // 질풍참
        public void DashAttack()
        {
            if (_isDashing || !_cooldownManager.IsReady(DashCooldownKey, DashCooldown)) return;
            StartCoroutine(DashCoroutine());
        }

        private IEnumerator DashCoroutine()
        {
            _isDashing = true;
            _cooldownManager.Use(DashCooldownKey);
            OnDashStarted?.Invoke();

            Vector3 dashDirection = _cameraTransform.forward;
            float dashSpeed = DashDistance / _dashDuration;

            float elapsed = 0f;
            while (elapsed < _dashDuration)
            {
                _controller.Move(dashDirection * dashSpeed * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }

            _isDashing = false;
            OnDashEnded?.Invoke();
        }

        public void ResetDashCooldown() => _cooldownManager.Reset(DashCooldownKey);
    }
}