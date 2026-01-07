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
        }

        // 이동 방향으로 회전
        private void RotateToMoveDirection(Vector3 moveDirection)
        {
            if (moveDirection.sqrMagnitude < 0.01f) return;

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }

        /// <summary>
        /// 카메라 방향으로 즉시 회전 (공격/스킬 사용 시)
        /// </summary>
        public void RotateToCamera()
        {
            if (_cameraTransform == null) return;

            Vector3 forward = _cameraTransform.forward;
            forward.y = 0f;
            forward.Normalize();

            if (forward.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(forward);
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

            Vector3 moveDirection = GetWorldMoveDirection(input);
            _controller.Move(moveDirection * MoveSpeed * Time.deltaTime);

            // 이동 방향으로 회전
            RotateToMoveDirection(moveDirection);
        }

        /// <summary>
        /// 월드 이동 방향 계산 (카메라 기준)
        /// </summary>
        public Vector3 GetWorldMoveDirection(Vector2 input)
        {
            if (input.sqrMagnitude < 0.01f)
                return Vector3.zero;

            Vector3 forward = _cameraTransform.forward;
            Vector3 right = _cameraTransform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            return (forward * input.y + right * input.x).normalized;
        }

        /// <summary>
        /// 캐릭터 로컬 기준 속도 계산 (Blend Tree용, 8방향)
        /// </summary>
        public Vector2 GetLocalVelocity(Vector2 input)
        {
            if (input.sqrMagnitude < 0.01f)
                return Vector2.zero;

            Vector3 worldDir = GetWorldMoveDirection(input);
            Vector3 localDir = transform.InverseTransformDirection(worldDir);

            return new Vector2(localDir.x, localDir.z);
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