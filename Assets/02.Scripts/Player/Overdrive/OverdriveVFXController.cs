using UnityEngine;
using _02.Scripts.Player.Gauge;
using _02.Scripts.CameraFX;
using _02.Scripts.Player.Combat;
using Drakkar.GameUtils;

namespace _02.Scripts.Player.Overdrive
{
    /// <summary>
    /// Overdrive 모드의 시각 효과 관리 (간소화 버전)
    /// - 카메라 효과 (FOV, Shake)
    /// - Trail 색상 변경
    /// </summary>
    public class OverdriveVFXController : MonoBehaviour
    {
        public static OverdriveVFXController Instance { get; private set; }

        [Header("References")]
        [SerializeField] private GaugeManager _gaugeManager;

        [Header("Camera Settings")]
        [SerializeField] private float _activationFOVIncrease = 8f;
        [SerializeField] private float _fovTransitionTime = 0.2f;
        [SerializeField] private float _activationShakeIntensity = 0.5f;
        [SerializeField] private float _deactivationShakeIntensity = 0.3f;

        [Header("Trail Effects")]
        [SerializeField] private Material _normalTrailMaterial;
        [SerializeField] private Material _overdriveTrailMaterial;
        [SerializeField] private DrakkarTrail[] _playerTrails;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void OnEnable()
        {
            if (_gaugeManager != null)
            {
                _gaugeManager.OnOverDriveActivated += HandleOverdriveActivated;
                _gaugeManager.OnOverDriveDeactivated += HandleOverdriveDeactivated;
            }
        }

        private void OnDisable()
        {
            if (_gaugeManager != null)
            {
                _gaugeManager.OnOverDriveActivated -= HandleOverdriveActivated;
                _gaugeManager.OnOverDriveDeactivated -= HandleOverdriveDeactivated;
            }
        }

        #region Event Handlers

        private void HandleOverdriveActivated()
        {
            // 카메라 FOV 확대
            CameraEffectsManager.Instance?.StartOverdriveFOV(
                _activationFOVIncrease,
                _fovTransitionTime);

            // 카메라 셰이크
            CameraShakeManager.Instance?.Shake(_activationShakeIntensity);

            // Trail 색상 변경
            ApplyOverdriveTrailMaterial();

            // Hit Effect 프리팹 변경
            HitEffectPool.Instance?.SetOverdriveActive(true);
        }

        private void HandleOverdriveDeactivated()
        {
            // 카메라 FOV 복귀
            CameraEffectsManager.Instance?.EndOverdriveFOV(_fovTransitionTime);

            // 카메라 셰이크
            CameraShakeManager.Instance?.Shake(_deactivationShakeIntensity);

            // Trail 색상 복귀
            RevertToNormalTrailMaterial();

            // Hit Effect 프리팹 복귀
            HitEffectPool.Instance?.SetOverdriveActive(false);
        }

        #endregion

        #region Trail Effects

        private void ApplyOverdriveTrailMaterial()
        {
            if (_overdriveTrailMaterial == null || _playerTrails == null) return;

            foreach (var trail in _playerTrails)
            {
                if (trail != null)
                    trail.TrailMaterial = _overdriveTrailMaterial;
            }
        }

        private void RevertToNormalTrailMaterial()
        {
            if (_normalTrailMaterial == null || _playerTrails == null) return;

            foreach (var trail in _playerTrails)
            {
                if (trail != null)
                    trail.TrailMaterial = _normalTrailMaterial;
            }
        }

        #endregion

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            // Trail 복귀
            RevertToNormalTrailMaterial();
        }
    }
}