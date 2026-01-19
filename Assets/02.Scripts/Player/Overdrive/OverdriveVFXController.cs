using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using _02.Scripts.Player.Gauge;
using _02.Scripts.CameraFX;
using _02.Scripts.Player.Combat;
using Drakkar.GameUtils;

namespace _02.Scripts.Player.Overdrive
{
    /// <summary>
    /// Overdrive 모드의 모든 시각 효과를 통합 관리
    /// 스텔라 블레이드 타키모드 스타일 구현
    /// </summary>
    public class OverdriveVFXController : MonoBehaviour
    {
        public static OverdriveVFXController Instance { get; private set; }

        [Header("References")]
        [SerializeField] private GaugeManager _gaugeManager;
        [SerializeField] private OverdriveVFXSettings _settings;
        [SerializeField] private Volume _postProcessVolume;

        [Header("Screen Flash")]
        [SerializeField] private CanvasGroup _flashCanvasGroup;

        [Header("Trail Effects")]
        [SerializeField] private Material _normalTrailMaterial;
        [SerializeField] private Material _overdriveTrailMaterial;
        [SerializeField] private DrakkarTrail[] _playerTrails;

        [Header("Weapon Emission")]
        [SerializeField] private Renderer[] _weaponRenderers;
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private MaterialPropertyBlock _weaponPropertyBlock;
        private Color[] _originalEmissionColors;

        [Header("Afterimage Effect")]
        [SerializeField] private AfterimageController _afterimageController;

        [Header("Particle Effects (Optional)")]
        [SerializeField] private ParticleSystem _activationFlashVFX;
        [SerializeField] private ParticleSystem _auraVFX;
        [SerializeField] private ParticleSystem _deactivationShockwaveVFX;
        [SerializeField] private ParticleSystem _groundEnergyVFX;

        // Post Processing Components
        private Bloom _bloom;
        private ChromaticAberration _chromaticAberration;
        private Vignette _vignette;
        private ColorAdjustments _colorAdjustments;

        // 기본값 저장
        private float _baseBloomIntensity;
        private float _baseChromaticAberration;
        private float _baseVignetteIntensity;
        private float _baseSaturation;
        private float _baseContrast;

        // 상태
        private bool _isOverdriveActive;
        private Coroutine _activationCoroutine;
        private Coroutine _deactivationCoroutine;
        private Coroutine _pulseCoroutine;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }

            InitializePostProcessing();
            InitializeWeaponEmission();
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

        private void InitializePostProcessing()
        {
            if (_postProcessVolume == null) return;

            // Bloom
            if (_postProcessVolume.profile.TryGet(out _bloom))
                _baseBloomIntensity = _bloom.intensity.value;

            // Chromatic Aberration
            if (_postProcessVolume.profile.TryGet(out _chromaticAberration))
                _baseChromaticAberration = _chromaticAberration.intensity.value;

            // Vignette
            if (_postProcessVolume.profile.TryGet(out _vignette))
                _baseVignetteIntensity = _vignette.intensity.value;

            // Color Adjustments
            if (_postProcessVolume.profile.TryGet(out _colorAdjustments))
            {
                _baseSaturation = _colorAdjustments.saturation.value;
                _baseContrast = _colorAdjustments.contrast.value;
            }
        }

        #region Event Handlers

        private void HandleOverdriveActivated()
        {
            if (_deactivationCoroutine != null)
            {
                StopCoroutine(_deactivationCoroutine);
                _deactivationCoroutine = null;
            }

            _activationCoroutine = StartCoroutine(ActivationSequence());
        }

        private void HandleOverdriveDeactivated()
        {
            if (_activationCoroutine != null)
            {
                StopCoroutine(_activationCoroutine);
                _activationCoroutine = null;
            }

            if (_pulseCoroutine != null)
            {
                StopCoroutine(_pulseCoroutine);
                _pulseCoroutine = null;
            }

            _deactivationCoroutine = StartCoroutine(DeactivationSequence());
        }

        #endregion

        #region Activation Sequence

        private IEnumerator ActivationSequence()
        {
            _isOverdriveActive = true;

            // 1. 동시 시작: 플래시 + 슬로우모션 + 카메라 효과
            StartCoroutine(PlayActivationFlash());
            StartCoroutine(PlayActivationSlowMotion());

            // 카메라 줌아웃
            CameraEffectsManager.Instance?.StartOverdriveFOV(
                _settings.ActivationFOVIncrease,
                _settings.ActivationFOVTransitionTime);

            // 카메라 셰이크
            CameraShakeManager.Instance?.Shake(_settings.ActivationShakeIntensity);

            // 2. Post Processing 전환 (페이드인)
            StartCoroutine(TransitionPostProcessing(true));

            // 3. Trail 색상 변경 + 무기 발광
            ApplyOverdriveTrailMaterial();
            StartCoroutine(TransitionWeaponEmission(true));

            // 4. 파티클 VFX (설정된 경우)
            if (_activationFlashVFX != null)
                _activationFlashVFX.Play();

            if (_auraVFX != null)
                _auraVFX.Play();

            if (_groundEnergyVFX != null)
                _groundEnergyVFX.Play();

            // 5. 잔상 효과 시작
            if (_afterimageController != null)
                _afterimageController.StartAfterimage();

            // 슬로우모션 종료 대기
            yield return new WaitForSecondsRealtime(_settings.ActivationSlowDuration);

            // 6. 지속 효과 시작 (펄스)
            _pulseCoroutine = StartCoroutine(PulseEffect());

            _activationCoroutine = null;
        }

        private IEnumerator PlayActivationFlash()
        {
            if (_flashCanvasGroup == null) yield break;

            float elapsed = 0f;
            float halfDuration = _settings.FlashDuration / 2f;

            // 플래시 인
            while (elapsed < halfDuration)
            {
                _flashCanvasGroup.alpha = Mathf.Lerp(0f, _settings.FlashColor.a, elapsed / halfDuration);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            // 플래시 아웃
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                _flashCanvasGroup.alpha = Mathf.Lerp(_settings.FlashColor.a, 0f, elapsed / halfDuration);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            _flashCanvasGroup.alpha = 0f;
        }

        private IEnumerator PlayActivationSlowMotion()
        {
            // 슬로우모션 시작
            Time.timeScale = _settings.ActivationTimeScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;

            yield return new WaitForSecondsRealtime(_settings.ActivationSlowDuration);

            // 빠르게 복귀 (타격감 부여)
            float elapsed = 0f;
            while (elapsed < _settings.TimeScaleRecoveryDuration)
            {
                float t = elapsed / _settings.TimeScaleRecoveryDuration;
                Time.timeScale = Mathf.Lerp(_settings.ActivationTimeScale, 1f, t);
                Time.fixedDeltaTime = 0.02f * Time.timeScale;
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }

        #endregion

        #region Deactivation Sequence

        private IEnumerator DeactivationSequence()
        {
            // 1. 짧은 슬로우모션
            StartCoroutine(PlayDeactivationSlowMotion());

            // 2. 카메라 셰이크
            CameraShakeManager.Instance?.Shake(_settings.DeactivationShakeIntensity);

            // 3. 쇼크웨이브 VFX (설정된 경우)
            if (_deactivationShockwaveVFX != null)
                _deactivationShockwaveVFX.Play();

            // 4. Post Processing 페이드아웃
            StartCoroutine(TransitionPostProcessing(false));

            // 5. Trail 색상 복귀 + 무기 발광 해제
            StartCoroutine(TransitionWeaponEmission(false));

            // 6. 카메라 FOV 복귀
            CameraEffectsManager.Instance?.EndOverdriveFOV(_settings.DeactivationFadeDuration);

            // 7. 오라 VFX 종료
            if (_auraVFX != null)
                _auraVFX.Stop();

            if (_groundEnergyVFX != null)
                _groundEnergyVFX.Stop();

            // 8. 잔상 효과 종료
            if (_afterimageController != null)
                _afterimageController.StopAfterimage();

            yield return new WaitForSeconds(_settings.DeactivationFadeDuration);

            // Trail은 페이드아웃 후 복귀
            RevertToNormalTrailMaterial();

            _isOverdriveActive = false;
            _deactivationCoroutine = null;
        }

        private IEnumerator PlayDeactivationSlowMotion()
        {
            Time.timeScale = _settings.DeactivationTimeScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;

            yield return new WaitForSecondsRealtime(_settings.DeactivationSlowDuration);

            // 부드럽게 복귀
            float elapsed = 0f;
            float recoveryDuration = 0.15f;
            while (elapsed < recoveryDuration)
            {
                float t = elapsed / recoveryDuration;
                Time.timeScale = Mathf.Lerp(_settings.DeactivationTimeScale, 1f, t);
                Time.fixedDeltaTime = 0.02f * Time.timeScale;
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }

        #endregion

        #region Post Processing

        private IEnumerator TransitionPostProcessing(bool toOverdrive)
        {
            float elapsed = 0f;
            float duration = _settings.TransitionDuration;

            // 시작값과 목표값 계산
            float startBloom = toOverdrive ? _baseBloomIntensity : _baseBloomIntensity + _settings.BloomIntensityBoost;
            float endBloom = toOverdrive ? _baseBloomIntensity + _settings.BloomIntensityBoost : _baseBloomIntensity;

            float startCA = toOverdrive ? _baseChromaticAberration : _settings.ChromaticAberration;
            float endCA = toOverdrive ? _settings.ChromaticAberration : _baseChromaticAberration;

            float startVignette = toOverdrive ? _baseVignetteIntensity : _settings.VignetteIntensity;
            float endVignette = toOverdrive ? _settings.VignetteIntensity : _baseVignetteIntensity;

            float startSaturation = toOverdrive ? _baseSaturation : _baseSaturation + _settings.SaturationBoost;
            float endSaturation = toOverdrive ? _baseSaturation + _settings.SaturationBoost : _baseSaturation;

            float startContrast = toOverdrive ? _baseContrast : _baseContrast + _settings.ContrastBoost;
            float endContrast = toOverdrive ? _baseContrast + _settings.ContrastBoost : _baseContrast;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                t = Mathf.SmoothStep(0f, 1f, t); // Ease in-out

                if (_bloom != null)
                    _bloom.intensity.value = Mathf.Lerp(startBloom, endBloom, t);

                if (_chromaticAberration != null)
                    _chromaticAberration.intensity.value = Mathf.Lerp(startCA, endCA, t);

                if (_vignette != null)
                {
                    _vignette.intensity.value = Mathf.Lerp(startVignette, endVignette, t);
                    if (toOverdrive && t > 0.5f)
                        _vignette.color.value = _settings.VignetteColor;
                }

                if (_colorAdjustments != null)
                {
                    _colorAdjustments.saturation.value = Mathf.Lerp(startSaturation, endSaturation, t);
                    _colorAdjustments.contrast.value = Mathf.Lerp(startContrast, endContrast, t);
                }

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            // 최종값 설정
            if (_bloom != null)
                _bloom.intensity.value = endBloom;

            if (_chromaticAberration != null)
                _chromaticAberration.intensity.value = endCA;

            if (_vignette != null)
                _vignette.intensity.value = endVignette;

            if (_colorAdjustments != null)
            {
                _colorAdjustments.saturation.value = endSaturation;
                _colorAdjustments.contrast.value = endContrast;
            }
        }

        private IEnumerator PulseEffect()
        {
            while (_isOverdriveActive)
            {
                float pulseValue = Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f / _settings.PulsePeriod)
                    * _settings.PulseIntensity;

                if (_bloom != null)
                    _bloom.intensity.value = _baseBloomIntensity + _settings.BloomIntensityBoost + pulseValue;

                if (_vignette != null)
                    _vignette.intensity.value = _settings.VignetteIntensity + pulseValue * 0.5f;

                yield return null;
            }
        }

        #endregion

        #region Trail Effects

        private void ApplyOverdriveTrailMaterial()
        {
            if (_overdriveTrailMaterial == null) return;

            if (_playerTrails != null && _playerTrails.Length > 0)
            {
                foreach (var trail in _playerTrails)
                {
                    if (trail != null)
                        trail.TrailMaterial = _overdriveTrailMaterial;
                }
            }
        }

        private void RevertToNormalTrailMaterial()
        {
            if (_normalTrailMaterial == null) return;

            if (_playerTrails != null && _playerTrails.Length > 0)
            {
                foreach (var trail in _playerTrails)
                {
                    if (trail != null)
                        trail.TrailMaterial = _normalTrailMaterial;
                }
            }
        }

        #endregion

        #region Weapon Emission

        private void InitializeWeaponEmission()
        {
            _weaponPropertyBlock = new MaterialPropertyBlock();

            if (_weaponRenderers == null || _weaponRenderers.Length == 0) return;

            _originalEmissionColors = new Color[_weaponRenderers.Length];
            for (int i = 0; i < _weaponRenderers.Length; i++)
            {
                if (_weaponRenderers[i] != null && _weaponRenderers[i].sharedMaterial != null)
                {
                    if (_weaponRenderers[i].sharedMaterial.HasProperty(EmissionColor))
                        _originalEmissionColors[i] = _weaponRenderers[i].sharedMaterial.GetColor(EmissionColor);
                    else
                        _originalEmissionColors[i] = Color.black;
                }
            }
        }

        private IEnumerator TransitionWeaponEmission(bool toOverdrive)
        {
            if (_weaponRenderers == null || _weaponRenderers.Length == 0) yield break;

            float elapsed = 0f;
            float duration = _settings.TransitionDuration;

            Color targetColor = _settings.WeaponEmissionColor * _settings.WeaponEmissionMultiplier;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                t = Mathf.SmoothStep(0f, 1f, t);

                for (int i = 0; i < _weaponRenderers.Length; i++)
                {
                    if (_weaponRenderers[i] == null) continue;

                    Color startColor = toOverdrive ? _originalEmissionColors[i] : targetColor;
                    Color endColor = toOverdrive ? targetColor : _originalEmissionColors[i];
                    Color currentColor = Color.Lerp(startColor, endColor, t);

                    _weaponRenderers[i].GetPropertyBlock(_weaponPropertyBlock);
                    _weaponPropertyBlock.SetColor(EmissionColor, currentColor);
                    _weaponRenderers[i].SetPropertyBlock(_weaponPropertyBlock);
                }

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            // 최종값 설정
            for (int i = 0; i < _weaponRenderers.Length; i++)
            {
                if (_weaponRenderers[i] == null) continue;

                Color finalColor = toOverdrive ? targetColor : _originalEmissionColors[i];
                _weaponRenderers[i].GetPropertyBlock(_weaponPropertyBlock);
                _weaponPropertyBlock.SetColor(EmissionColor, finalColor);
                _weaponRenderers[i].SetPropertyBlock(_weaponPropertyBlock);
            }
        }

        private void ResetWeaponEmission()
        {
            if (_weaponRenderers == null || _originalEmissionColors == null) return;

            for (int i = 0; i < _weaponRenderers.Length; i++)
            {
                if (_weaponRenderers[i] == null) continue;

                _weaponRenderers[i].GetPropertyBlock(_weaponPropertyBlock);
                _weaponPropertyBlock.SetColor(EmissionColor, _originalEmissionColors[i]);
                _weaponRenderers[i].SetPropertyBlock(_weaponPropertyBlock);
            }
        }

        #endregion

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            // TimeScale 복구
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;

            // Post Processing 복구
            ResetPostProcessing();

            // Weapon Emission 복구
            ResetWeaponEmission();

            // Trail 복구
            RevertToNormalTrailMaterial();
        }

        private void ResetPostProcessing()
        {
            if (_bloom != null)
                _bloom.intensity.value = _baseBloomIntensity;

            if (_chromaticAberration != null)
                _chromaticAberration.intensity.value = _baseChromaticAberration;

            if (_vignette != null)
                _vignette.intensity.value = _baseVignetteIntensity;

            if (_colorAdjustments != null)
            {
                _colorAdjustments.saturation.value = _baseSaturation;
                _colorAdjustments.contrast.value = _baseContrast;
            }
        }
    }
}