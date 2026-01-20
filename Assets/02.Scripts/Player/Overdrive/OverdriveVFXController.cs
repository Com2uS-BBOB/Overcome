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

        [Header("Electricity VFX")]
        [SerializeField] private GameObject _electricityVFXPrefab;
        [SerializeField] private Transform _vfxSpawnPoint;
        [SerializeField] private float _vfxDuration = 0f; // 0 = 오버드라이브 종료 시까지 유지

        private GameObject _activeElectricityVFX;

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

            // 전기 이펙트 생성
            SpawnElectricityVFX();
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

            // 전기 이펙트 제거
            DestroyElectricityVFX();
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

        #region Electricity VFX

        private void SpawnElectricityVFX()
        {
            if (_electricityVFXPrefab == null) return;

            // 기존 VFX가 있으면 제거
            DestroyElectricityVFX();

            // Spawn Point가 없으면 자기 자신의 Transform 사용
            var spawnPoint = _vfxSpawnPoint != null ? _vfxSpawnPoint : transform;

            _activeElectricityVFX = Instantiate(
                _electricityVFXPrefab,
                spawnPoint.position,
                Quaternion.identity,
                spawnPoint // 플레이어를 따라다니도록 부모 설정
            );

            // 로컬 위치/회전 초기화
            _activeElectricityVFX.transform.localPosition = Vector3.zero;
            _activeElectricityVFX.transform.localRotation = Quaternion.identity;

            // 지속 시간이 설정되어 있으면 자동 제거
            if (_vfxDuration > 0f)
            {
                Destroy(_activeElectricityVFX, _vfxDuration);
            }
        }

        private void DestroyElectricityVFX()
        {
            if (_activeElectricityVFX != null)
            {
                Destroy(_activeElectricityVFX);
                _activeElectricityVFX = null;
            }
        }

        #endregion

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            // Trail 복귀
            RevertToNormalTrailMaterial();

            // 전기 이펙트 정리
            DestroyElectricityVFX();
        }
    }
}