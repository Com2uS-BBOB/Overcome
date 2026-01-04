using System;
using System.Collections;
using UnityEngine;
using _02.Scripts.Common;
using _02.Scripts.Player.Data;

namespace _02.Scripts.Player.Movement
{
    /// <summary>
    /// 플레이어 이동 처리
    /// CharacterController 기반, 카메라 방향 기준 이동
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        private const string DashCooldownKey = "Dash";

        [Header("Settings (고정값)")]
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

        // Dash 상태
        private bool _isDashing;

        // 스탯 참조
        private float MoveSpeed => _stats != null ? _stats.MoveSpeed : 8f;
        private float JumpForce => _stats != null ? _stats.JumpForce : 10f;
        private float DashDistance => _stats != null ? _stats.DashDistance : 10f;
        private float DashCooldown => _stats != null ? _stats.DashCooldown : 3f;

        public bool IsGrounded => _isGrounded;
        public bool IsMoving { get; private set; }
        public bool IsDashing => _isDashing;

        /// 질풍참 가능 여부
        public bool CanDash => !_isDashing && _cooldownManager.IsReady(DashCooldownKey, DashCooldown);

        // 이벤트
        public event Action OnDashStarted;
        public event Action OnDashEnded;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _stats = GetComponent<PlayerRuntimeStats>();
            _cooldownManager = new CooldownManager();

            if (_cameraTransform == null)
            {
                _cameraTransform = Camera.main?.transform;
            }

            if (_stats == null)
            {
                Debug.LogWarning("[PlayerMovement] RuntimeStats 없음 - 기본값 사용");
            }
        }

        /// <summary>
        /// 외부에서 Stats 주입 (PlayerController에서 호출)
        /// </summary>
        public void Initialize(PlayerRuntimeStats stats)
        {
            _stats = stats;
        }

        private void Update()
        {
            CheckGround();
            ApplyGravity();
            RotateToCamera();
        }

        /// <summary>
        /// 항상 카메라 방향으로 회전
        /// </summary>
        private void RotateToCamera()
        {
            if (_cameraTransform == null) return;

            Vector3 forward = _cameraTransform.forward;
            forward.y = 0f;
            forward.Normalize();

            if (forward.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(forward);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    _rotationSpeed * Time.deltaTime
                );
            }
        }

        /// <summary>
        /// 입력값 기반 이동 처리
        /// </summary>
        public void Move(Vector2 input)
        {
            if (input.sqrMagnitude < 0.01f)
            {
                IsMoving = false;
                return;
            }

            IsMoving = true;

            // 카메라 기준 이동 방향 계산
            Vector3 forward = _cameraTransform.forward;
            Vector3 right = _cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 moveDirection = forward * input.y + right * input.x;

            // 이동 적용
            _controller.Move(moveDirection * MoveSpeed * Time.deltaTime);
        }

        /// <summary>
        /// 점프 실행
        /// </summary>
        public void Jump()
        {
            // Input 이벤트 타이밍 문제로 controller.isGrounded 직접 체크
            bool canJump = _isGrounded || _controller.isGrounded;

            if (!canJump)
            {
                Debug.Log("[Movement] 점프 불가 - 공중");
                return;
            }

            Debug.Log("[Movement] 점프!");
            _velocity.y = JumpForce;
        }

        /// <summary>
        /// 지면 체크
        /// </summary>
        private void CheckGround()
        {
            // CharacterController 기본 체크
            _isGrounded = _controller.isGrounded;

            // SphereCast로 더 넓은 범위 체크 (이동 중에도 안정적)
            if (!_isGrounded)
            {
                _isGrounded = Physics.SphereCast(
                    transform.position + Vector3.up * (_controller.radius + 0.1f),
                    _controller.radius * 0.9f,
                    Vector3.down,
                    out _,
                    _groundCheckDistance + 0.1f,
                    _groundLayer
                );
            }

            // 레이어 미설정 시 기본 레이캐스트 (모든 레이어)
            if (!_isGrounded && _groundLayer == 0)
            {
                _isGrounded = Physics.Raycast(
                    transform.position + Vector3.up * 0.1f,
                    Vector3.down,
                    _groundCheckDistance + 0.2f
                );
            }

            // 바닥에 있을 때 y 속도 리셋
            if (_isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }
        }

        /// <summary>
        /// 중력 적용
        /// </summary>
        private void ApplyGravity()
        {
            _velocity.y += _gravity * Time.deltaTime;
            _controller.Move(_velocity * Time.deltaTime);
        }

        #region Dash Attack (질풍참)

        /// <summary>
        /// 질풍참 실행
        /// </summary>
        public void DashAttack()
        {
            if (_isDashing)
            {
                Debug.Log("[Movement] 질풍참 불가 - 이미 대시 중");
                return;
            }

            if (!_cooldownManager.IsReady(DashCooldownKey, DashCooldown))
            {
                float remaining = _cooldownManager.GetRemainingTime(DashCooldownKey, DashCooldown);
                Debug.Log($"[Movement] 질풍참 불가 - 쿨타임 {remaining:F1}초 남음");
                return;
            }

            StartCoroutine(DashCoroutine());
        }

        private IEnumerator DashCoroutine()
        {
            _isDashing = true;
            _cooldownManager.Use(DashCooldownKey);
            OnDashStarted?.Invoke();

            Debug.Log("[Movement] 질풍참 시작!");

            // 대시 방향 (카메라 방향 - 위/아래 포함)
            Vector3 dashDirection = _cameraTransform.forward;
            float dashSpeed = DashDistance / _dashDuration;

            float elapsed = 0f;
            while (elapsed < _dashDuration)
            {
                // 대시 이동
                _controller.Move(dashDirection * dashSpeed * Time.deltaTime);

                elapsed += Time.deltaTime;
                yield return null;
            }

            _isDashing = false;
            OnDashEnded?.Invoke();

            Debug.Log("[Movement] 질풍참 종료!");
        }

        /// <summary>
        /// 질풍참 쿨타임 초기화 (적 처치 시)
        /// </summary>
        public void ResetDashCooldown()
        {
            _cooldownManager.Reset(DashCooldownKey);
            Debug.Log("[Movement] 질풍참 쿨타임 초기화!");
        }

        #endregion

        private void OnDrawGizmosSelected()
        {
            // Ground Check 시각화
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawLine(
                transform.position + Vector3.up * 0.1f,
                transform.position + Vector3.down * _groundCheckDistance
            );
        }
    }
}