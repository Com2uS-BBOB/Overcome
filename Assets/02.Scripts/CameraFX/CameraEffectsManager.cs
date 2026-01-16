using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

namespace _02.Scripts.CameraFX
{
    /// <summary>
    /// 카메라 효과 관리자 (줌 펀치, FOV 조절)
    /// </summary>
    public class CameraEffectsManager : MonoBehaviour
    {
        public static CameraEffectsManager Instance { get; private set; }

        [Header("Cinemachine")]
        [SerializeField] private CinemachineCamera _virtualCamera;

        [Header("Zoom Punch Settings")]
        [SerializeField] private float _zoomAmount = 3f;
        [SerializeField] private float _zoomDuration = 0.1f;

        [Header("Dash FOV Settings")]
        [SerializeField] private float _dashFOVIncrease = 8f;
        [SerializeField] private float _dashFOVTransitionTime = 0.15f;

        private float _baseFOV;
        private Coroutine _zoomCoroutine;
        private Coroutine _dashFOVCoroutine;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                if (_virtualCamera != null)
                    _baseFOV = _virtualCamera.Lens.FieldOfView;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 타격 시 줌 펀치 (FOV 줌인 후 복귀)
        /// </summary>
        public void ZoomPunch()
        {
            if (_virtualCamera == null) return;

            if (_zoomCoroutine != null)
                StopCoroutine(_zoomCoroutine);

            _zoomCoroutine = StartCoroutine(ZoomPunchCoroutine());
        }

        private IEnumerator ZoomPunchCoroutine()
        {
            float half = _zoomDuration / 2f;
            float targetFOV = _baseFOV - _zoomAmount;

            // 줌인
            float elapsed = 0f;
            while (elapsed < half)
            {
                _virtualCamera.Lens.FieldOfView = Mathf.Lerp(_baseFOV, targetFOV, elapsed / half);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            // 줌아웃
            elapsed = 0f;
            while (elapsed < half)
            {
                _virtualCamera.Lens.FieldOfView = Mathf.Lerp(targetFOV, _baseFOV, elapsed / half);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            _virtualCamera.Lens.FieldOfView = _baseFOV;
            _zoomCoroutine = null;
        }

        /// <summary>
        /// 질풍참 시작 - FOV 확대
        /// </summary>
        public void StartDashFOV()
        {
            if (_virtualCamera == null) return;

            if (_dashFOVCoroutine != null)
                StopCoroutine(_dashFOVCoroutine);

            _dashFOVCoroutine = StartCoroutine(TransitionFOV(_baseFOV + _dashFOVIncrease));
        }

        /// <summary>
        /// 질풍참 종료 - FOV 복귀
        /// </summary>
        public void EndDashFOV()
        {
            if (_virtualCamera == null) return;

            if (_dashFOVCoroutine != null)
                StopCoroutine(_dashFOVCoroutine);

            _dashFOVCoroutine = StartCoroutine(TransitionFOV(_baseFOV));
        }

        private IEnumerator TransitionFOV(float targetFOV)
        {
            float startFOV = _virtualCamera.Lens.FieldOfView;
            float elapsed = 0f;

            while (elapsed < _dashFOVTransitionTime)
            {
                _virtualCamera.Lens.FieldOfView = Mathf.Lerp(startFOV, targetFOV, elapsed / _dashFOVTransitionTime);
                elapsed += Time.deltaTime;
                yield return null;
            }

            _virtualCamera.Lens.FieldOfView = targetFOV;
            _dashFOVCoroutine = null;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}