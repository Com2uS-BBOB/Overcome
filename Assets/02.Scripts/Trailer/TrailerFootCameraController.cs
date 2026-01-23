using UnityEngine;
using Unity.Cinemachine;

namespace _02.Scripts.Trailer
{
    /// <summary>
    /// 트레일러용 발 추적 카메라 컨트롤러
    /// 플레이어 걷기 모션 + 발쪽 로우앵글 카메라
    /// </summary>
    public class TrailerFootCameraController : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField] private CinemachineCamera _footCamera;
        [SerializeField] private int _activePriority = 20;
        [SerializeField] private int _inactivePriority = 0;

        [Header("Follow Target")]
        [SerializeField] private TrailerFootTarget _footTarget;

        [Header("Camera Settings")]
        [SerializeField] private Vector3 _followOffset = new Vector3(0f, 0.3f, -1.5f);
        [SerializeField] private float _fieldOfView = 50f;

        [Header("Player")]
        [SerializeField] private Transform _playerTransform;

        [Header("Walk Control")]
        [SerializeField] private bool _autoWalk = true;
        [SerializeField] private bool _playOnStart = true;
        [SerializeField] private float _walkSpeed = 2f;
        [SerializeField] private Vector3 _walkDirection = Vector3.forward;

        private bool _isPlaying;

        private void Start()
        {
            // Play 누르면 바로 시작
            if (_playOnStart)
            {
                Play();
            }
        }

        private void Update()
        {
            if (!_isPlaying || !_autoWalk) return;
            if (_playerTransform == null) return;

            // 플레이어 이동 (Transform 직접 이동)
            Vector3 moveDir = _playerTransform.TransformDirection(_walkDirection.normalized);
            _playerTransform.position += moveDir * _walkSpeed * Time.deltaTime;
        }

        /// <summary>
        /// 발 추적 카메라 연출 시작
        /// </summary>
        [ContextMenu("Play Foot Shot")]
        public void Play()
        {
            _isPlaying = true;

            // 카메라 활성화
            if (_footCamera != null)
                _footCamera.Priority = _activePriority;

            // 타겟 즉시 동기화
            if (_footTarget != null)
                _footTarget.Snap();
        }

        /// <summary>
        /// 연출 정지
        /// </summary>
        [ContextMenu("Stop Foot Shot")]
        public void Stop()
        {
            _isPlaying = false;

            // 카메라 비활성화
            if (_footCamera != null)
                _footCamera.Priority = _inactivePriority;
        }

#if UNITY_EDITOR
        [Header("Editor")]
        [SerializeField] private bool _setupOnValidate = true;

        private void OnValidate()
        {
            if (!_setupOnValidate) return;

            // FootTarget 자동 연결
            if (_footTarget == null)
                _footTarget = FindObjectOfType<TrailerFootTarget>();

            // 카메라 설정 적용
            if (_footCamera != null)
            {
                _footCamera.Lens.FieldOfView = _fieldOfView;

                // Follow 컴포넌트 설정
                var follow = _footCamera.GetComponent<CinemachineFollow>();
                if (follow != null)
                {
                    follow.FollowOffset = _followOffset;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_playerTransform == null) return;

            // 걷기 방향 표시
            Gizmos.color = Color.green;
            Vector3 start = _playerTransform.position;
            Vector3 end = start + _playerTransform.TransformDirection(_walkDirection.normalized) * 2f;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawSphere(end, 0.1f);
        }
#endif
    }
}