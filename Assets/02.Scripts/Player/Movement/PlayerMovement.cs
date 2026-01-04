using UnityEngine;

namespace _02.Scripts.Player.Movement
{
    /// <summary>
    /// 플레이어 이동 처리
    /// CharacterController 기반, 카메라 방향 기준 이동
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 8f;
        [SerializeField] private float _rotationSpeed = 10f;

        [Header("Jump")]
        [SerializeField] private float _jumpForce = 10f;

        [Header("Gravity")]
        [SerializeField] private float _gravity = -20f;
        [SerializeField] private float _groundCheckDistance = 0.2f;
        [SerializeField] private LayerMask _groundLayer;

        [Header("References")]
        [SerializeField] private Transform _cameraTransform;

        private CharacterController _controller;
        private Vector3 _velocity;
        private bool _isGrounded;

        public bool IsGrounded => _isGrounded;
        public bool IsMoving { get; private set; }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();

            if (_cameraTransform == null)
            {
                _cameraTransform = Camera.main?.transform;
            }
        }

        private void Update()
        {
            CheckGround();
            ApplyGravity();
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

            // 캐릭터 회전
            if (moveDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    _rotationSpeed * Time.deltaTime
                );
            }

            // 이동 적용
            _controller.Move(moveDirection * _moveSpeed * Time.deltaTime);
        }

        /// <summary>
        /// 점프 실행
        /// </summary>
        public void Jump()
        {
            if (!_isGrounded)
            {
                Debug.Log("[Movement] 점프 불가 - 공중");
                return;
            }

            Debug.Log("[Movement] 점프!");
            _velocity.y = _jumpForce;
        }

        /// <summary>
        /// 지면 체크
        /// </summary>
        private void CheckGround()
        {
            _isGrounded = _controller.isGrounded;

            // 추가 레이캐스트 체크 (더 정확한 감지)
            if (!_isGrounded)
            {
                _isGrounded = Physics.Raycast(
                    transform.position + Vector3.up * 0.1f,
                    Vector3.down,
                    _groundCheckDistance + 0.1f,
                    _groundLayer
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

        /// <summary>
        /// 이동 속도 변경
        /// </summary>
        public void SetMoveSpeed(float speed)
        {
            _moveSpeed = speed;
        }

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