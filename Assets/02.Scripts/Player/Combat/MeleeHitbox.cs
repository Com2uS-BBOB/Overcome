using UnityEngine;
using _02.Scripts.Player.Common;

namespace _02.Scripts.Player.Combat
{
    // 근접 공격 히트박스 (카메라 방향 추적 지원)
    public class MeleeHitbox : HitboxBase
    {
        [Header("Camera Direction Tracking")]
        [SerializeField] private bool _trackCameraDirection = true;
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private bool _trackContinuously = false;  // true: 활성화 중 계속 추적

        private Collider _collider;
        private Quaternion _originalLocalRotation;
        private Transform _playerTransform;

        protected override void Awake()
        {
            base.Awake();
            _collider = GetComponent<Collider>();
            _spawnHitEffect = true;
            _originalLocalRotation = transform.localRotation;
            _playerTransform = transform.root;

            if (_cameraTransform == null)
                _cameraTransform = Camera.main?.transform;

            DisableHitDetection();
        }

        public override void EnableHitDetection(float damage)
        {
            base.EnableHitDetection(damage);
            _collider.enabled = true;

            if (_trackCameraDirection && _cameraTransform != null)
                AlignToCameraDirection();
        }

        public override void DisableHitDetection()
        {
            base.DisableHitDetection();
            if (_collider != null) _collider.enabled = false;

            // 원래 로컬 회전으로 복원
            transform.localRotation = _originalLocalRotation;
        }

        private void Update()
        {
            // 활성화 중 지속 추적 (선택적)
            if (_isActive && _trackContinuously && _trackCameraDirection && _cameraTransform != null)
                AlignToCameraDirection();
        }

        /// <summary>
        /// Hitbox를 카메라가 바라보는 방향(Pitch 포함)으로 회전
        /// </summary>
        private void AlignToCameraDirection()
        {
            Vector3 cameraForward = _cameraTransform.forward;

            // 카메라 방향을 월드 좌표로 Hitbox에 적용
            // 플레이어의 Y축 회전 + 카메라의 Pitch를 조합
            if (cameraForward.sqrMagnitude > 0.01f)
            {
                // Hitbox를 카메라 방향으로 회전 (월드 기준)
                Quaternion targetWorldRotation = Quaternion.LookRotation(cameraForward);

                // 월드 회전을 로컬 회전으로 변환
                transform.rotation = targetWorldRotation;
            }
        }

        /// <summary>
        /// 카메라 Transform 설정 (외부에서 주입)
        /// </summary>
        public void SetCameraTransform(Transform cameraTransform)
        {
            _cameraTransform = cameraTransform;
        }

        // 같은 root 무시
        protected override bool ShouldIgnore(Collider other) => other.transform.root == transform.root;
    }
}