using System;
using UnityEngine;
using _02.Scripts.Player.Data;

namespace _02.Scripts.Player.Movement
{
    // 플레이어 이동 (CharacterController 기반)
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _rotationSpeed = 10f;

        [Header("Gravity")]
        [SerializeField] private float _gravity = -30f;

        [Header("References")]
        [SerializeField] private Transform _cameraTransform;

        private CharacterController _controller;
        private PlayerStats _stats;
        private Vector3 _velocity;
        private bool _isGrounded;

        private float MoveSpeed => _stats != null ? _stats.MoveSpeed : 8f;
        private float JumpForce => _stats != null ? _stats.JumpForce : 10f;

        public bool IsGrounded => _isGrounded;
        public bool IsMoving { get; private set; }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _stats = GetComponent<PlayerStats>();

            if (_cameraTransform == null) _cameraTransform = Camera.main?.transform;
        }

        public void Initialize(PlayerStats stats) => _stats = stats;

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

            if (_isGrounded && _velocity.y < 0)
                _velocity.y = -2f;
        }

        private void ApplyGravity()
        {
            _velocity.y += _gravity * Time.deltaTime;
            _controller.Move(_velocity * Time.deltaTime);
        }
    }
}