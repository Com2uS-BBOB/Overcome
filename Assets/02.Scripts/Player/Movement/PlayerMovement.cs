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
        [SerializeField] private float _groundedGravity = -5f;  // 지면에서 더 강하게 붙이기

        [Header("Ground Check")]
        [SerializeField] private float _groundCheckDistance = 0.3f;
        [SerializeField] private float _groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask _groundLayer = ~0;  // 기본: 모든 레이어
        [SerializeField] private float _coyoteTime = 0.1f;  // 지면 떠난 후 점프 허용 시간

        [Header("References")]
        [SerializeField] private Transform _cameraTransform;

        private CharacterController _controller;
        private PlayerStats _stats;
        private Vector3 _velocity;          // 수직 속도 (점프/중력)
        private Vector3 _horizontalMove;    // 수평 이동
        private bool _isGrounded;
        private float _coyoteTimer;
        private int _jumpCount;             // 현재 점프 횟수
        private const int MaxJumpCount = 2; // 최대 점프 횟수 (2단 점프)

        // 중력 스케일 (공중 부유감 제어)
        private float _gravityScale = 1f;

        private float MoveSpeed => _stats != null ? _stats.MoveSpeed : 8f;
        private float JumpForce => _stats != null ? _stats.JumpForce : 10f;

        public bool IsGrounded => _isGrounded || _coyoteTimer > 0f;
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
            ApplyMovement();
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

        // 카메라 기준 이동 (수평 이동 저장, ApplyMovement에서 적용)
        public void Move(Vector2 input)
        {
            if (input.sqrMagnitude < 0.01f)
            {
                IsMoving = false;
                _horizontalMove = Vector3.zero;
                return;
            }

            IsMoving = true;

            Vector3 moveDirection = GetWorldMoveDirection(input);
            _horizontalMove = moveDirection * MoveSpeed;

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

        /// <summary>
        /// 점프 시도. 1단/2단 점프 가능 여부에 따라 결과 반환
        /// </summary>
        /// <returns>0: 점프 불가, 1: 1단 점프, 2: 2단 점프</returns>
        public int TryJump()
        {
            // 1단 점프: 지면에 있을 때
            if (IsGrounded)
            {
                _isGrounded = false;
                _coyoteTimer = 0f;
                _jumpCount = 1;
                _velocity.y = JumpForce;
                return 1;
            }

            // 2단 점프: 공중에서 1회만 가능
            if (_jumpCount < MaxJumpCount)
            {
                _jumpCount = MaxJumpCount;
                _velocity.y = JumpForce;
                return 2;
            }

            return 0;
        }

        [Obsolete("Use TryJump() instead")]
        public void Jump()
        {
            TryJump();
        }

        private void CheckGround()
        {
            // 상승 중에는 지면 체크 스킵 (점프 직후 지면 재감지 방지)
            if (_velocity.y > 0.1f)
            {
                _isGrounded = false;
                _coyoteTimer = 0f;
                return;
            }

            // Raycast로 안정적인 지면 체크 (CharacterController 아래에서 시작)
            float rayStart = _controller.height / 2f + _controller.skinWidth;
            Vector3 rayOrigin = transform.position + Vector3.up * rayStart;

            bool rayHit = Physics.Raycast(
                rayOrigin,
                Vector3.down,
                out _,
                rayStart + _groundCheckDistance,
                _groundLayer,
                QueryTriggerInteraction.Ignore
            );

            // CharacterController 결과와 병합
            bool groundCheck = rayHit || _controller.isGrounded;

            // Coyote Time 관리
            if (groundCheck)
            {
                _isGrounded = true;
                _coyoteTimer = _coyoteTime;
                _jumpCount = 0;  // 착지 시 점프 횟수 리셋
            }
            else
            {
                _coyoteTimer -= Time.deltaTime;
                _isGrounded = false;
            }

            // 지면에서 아래로 힘 적용 (붙어있게)
            if (_isGrounded && _velocity.y < 0)
                _velocity.y = _groundedGravity;
        }

        // 디버그용 Gizmo
        private void OnDrawGizmosSelected()
        {
            if (_controller == null) _controller = GetComponent<CharacterController>();
            if (_controller == null) return;

            float rayStart = _controller.height / 2f + _controller.skinWidth;
            Vector3 rayOrigin = transform.position + Vector3.up * rayStart;

            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * (rayStart + _groundCheckDistance));
            Gizmos.DrawWireSphere(rayOrigin + Vector3.down * (rayStart + _groundCheckDistance), 0.05f);
        }

        private void ApplyGravity()
        {
            if (!_isGrounded)
                _velocity.y += _gravity * _gravityScale * Time.deltaTime;
        }

        #region Gravity Scale (공중 부유감)

        /// <summary>
        /// 중력 스케일 설정 (1.0 = 기본, 0.3 = 부유감)
        /// </summary>
        public void SetGravityScale(float scale) => _gravityScale = scale;

        /// <summary>
        /// 중력 스케일 기본값으로 리셋
        /// </summary>
        public void ResetGravityScale() => _gravityScale = 1f;

        /// <summary>
        /// 수직 속도 추가 (체공 연장용)
        /// </summary>
        public void AddVerticalVelocity(float amount)
        {
            _velocity.y = Mathf.Max(_velocity.y, 0f) + amount;
        }

        #endregion

        /// <summary>
        /// 수평 이동 + 수직 속도를 합쳐서 한번에 적용
        /// </summary>
        private void ApplyMovement()
        {
            Vector3 finalMove = _horizontalMove + _velocity;
            _controller.Move(finalMove * Time.deltaTime);

            // 적용 후 수평 이동 리셋 (매 프레임 Move() 호출 필요)
            _horizontalMove = Vector3.zero;
        }
    }
}