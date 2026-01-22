using UnityEngine;

namespace _02.Scripts.Trailer
{
    /// <summary>
    /// 플레이어 발 위치를 추적하는 타겟
    /// Cinemachine 카메라의 Follow/LookAt 타겟으로 사용
    /// </summary>
    public class TrailerFootTarget : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private Vector3 _footOffset = new Vector3(0f, 0.1f, 0f);

        [Header("Smoothing")]
        [SerializeField] private float _smoothTime = 0.05f;

        private Vector3 _velocity;

        public Transform PlayerTransform
        {
            get => _playerTransform;
            set => _playerTransform = value;
        }

        private void Start()
        {
            if (_playerTransform == null)
                FindPlayer();
        }

        private void LateUpdate()
        {
            if (_playerTransform == null) return;

            Vector3 target = _playerTransform.position + _footOffset;
            transform.position = Vector3.SmoothDamp(transform.position, target, ref _velocity, _smoothTime);
        }

        public void FindPlayer()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                _playerTransform = player.transform;
        }

        /// <summary>
        /// 즉시 위치 동기화
        /// </summary>
        public void Snap()
        {
            if (_playerTransform == null) return;
            transform.position = _playerTransform.position + _footOffset;
            _velocity = Vector3.zero;
        }

        private void OnDrawGizmosSelected()
        {
            if (_playerTransform == null) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_playerTransform.position + _footOffset, 0.1f);
        }
    }
}