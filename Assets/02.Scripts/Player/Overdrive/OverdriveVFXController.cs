using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using _02.Scripts.Player.Gauge;
using _02.Scripts.CameraFX;
using _02.Scripts.Player.Combat;

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

        [Header("Particle Effects (Optional)")]
        [SerializeField] private ParticleSystem _activationFlashVFX;
        [SerializeField] private ParticleSystem _auraVFX;
        [SerializeField] private ParticleSystem _deactivationShockwaveVFX;

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

            // 3. 파티클 VFX (설정된 경우)
            if (_activationFlashVFX != null)
                _activationFlashVFX.Play();

            if (_auraVFX != null)
                _auraVFX.Play();

            // 슬로우모션 종료 대기
            yield return new WaitForSecondsRealtime(_settings.ActivationSlowDuration);

            // 4. 지속 효과 시작 (펄스)
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

            // 5. 카메라 FOV 복귀
            CameraEffectsManager.Instance?.EndOverdriveFOV(_settings.DeactivationFadeDuration);

            // 6. 오라 VFX 종료
            if (_auraVFX != null)
                _auraVFX.Stop();

            yield return new WaitForSeconds(_settings.DeactivationFadeDuration);

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

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            // TimeScale 복구
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;

            // Post Processing 복구
            ResetPostProcessing();
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