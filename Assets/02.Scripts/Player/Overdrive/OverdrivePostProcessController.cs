using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using _02.Scripts.Player.Gauge;

namespace _02.Scripts.Player.Overdrive
{
    /// <summary>
    /// Overdrive 모드의 Post Processing 효과 관리
    /// - 진입 펀치: Chromatic Aberration, Lens Distortion
    /// - 지속 효과: Bloom, Vignette, Color Adjustments
    /// </summary>
    public class OverdrivePostProcessController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GaugeManager _gaugeManager;
        [SerializeField] private Volume _volume;

        [Header("Entry Punch Settings")]
        [SerializeField] private float _punchDuration = 0.2f;
        [SerializeField] private float _punchChromaticPeak = 0.5f;
        [SerializeField] private float _punchLensDistortionPeak = -0.3f;

        [Header("Sustained Effect Settings")]
        [Tooltip("지속 중 Chromatic Aberration")]
        [SerializeField] private float _sustainedChromaticIntensity = 0.15f;

        [Tooltip("지속 중 Bloom 증가량")]
        [SerializeField] private float _bloomIntensityIncrease = 0.4f;

        [Tooltip("지속 중 Vignette 강도")]
        [SerializeField] private float _vignetteIntensity = 0.3f;
        [SerializeField] private Color _vignetteColor = new Color(0.6f, 0.05f, 0.1f, 1f);

        [Tooltip("Color Filter (붉은 톤)")]
        [SerializeField] private Color _colorFilter = new Color(1f, 0.88f, 0.85f, 1f);
        [SerializeField] private float _saturationIncrease = 20f;
        [SerializeField] private float _contrastIncrease = 10f;

        [Header("Transition Settings")]
        [SerializeField] private float _fadeInDuration = 0.3f;
        [SerializeField] private float _fadeOutDuration = 0.5f;

        // Volume Components
        private ChromaticAberration _chromaticAberration;
        private LensDistortion _lensDistortion;
        private Bloom _bloom;
        private Vignette _vignette;
        private ColorAdjustments _colorAdjustments;

        // Original Values
        private float _originalBloomIntensity;
        private float _originalVignetteIntensity;
        private Color _originalVignetteColor;
        private Color _originalColorFilter;
        private float _originalSaturation;
        private float _originalContrast;
        private float _originalChromaticIntensity;

        private Coroutine _effectCoroutine;
        private bool _isInitialized;

        private void Start()
        {
            InitializeComponents();
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

        private void InitializeComponents()
        {
            if (_volume == null || _volume.profile == null)
            {
                Debug.LogWarning("[OverdrivePostProcess] Volume 또는 Profile이 할당되지 않았습니다.");
                return;
            }

            var profile = _volume.profile;

            // Get or Add components
            if (!profile.TryGet(out _chromaticAberration))
            {
                _chromaticAberration = profile.Add<ChromaticAberration>(true);
            }

            if (!profile.TryGet(out _lensDistortion))
            {
                _lensDistortion = profile.Add<LensDistortion>(true);
            }

            if (!profile.TryGet(out _bloom))
            {
                _bloom = profile.Add<Bloom>(true);
            }

            if (!profile.TryGet(out _vignette))
            {
                _vignette = profile.Add<Vignette>(true);
            }

            if (!profile.TryGet(out _colorAdjustments))
            {
                _colorAdjustments = profile.Add<ColorAdjustments>(true);
            }

            // Store original values
            _originalBloomIntensity = _bloom.intensity.value;
            _originalVignetteIntensity = _vignette.intensity.value;
            _originalVignetteColor = _vignette.color.value;
            _originalColorFilter = _colorAdjustments.colorFilter.value;
            _originalSaturation = _colorAdjustments.saturation.value;
            _originalContrast = _colorAdjustments.contrast.value;
            _originalChromaticIntensity = _chromaticAberration.intensity.value;

            _isInitialized = true;
        }

        private void HandleOverdriveActivated()
        {
            if (!_isInitialized) return;

            if (_effectCoroutine != null)
                StopCoroutine(_effectCoroutine);

            _effectCoroutine = StartCoroutine(OverdriveActivationSequence());
        }

        private void HandleOverdriveDeactivated()
        {
            if (!_isInitialized) return;

            if (_effectCoroutine != null)
                StopCoroutine(_effectCoroutine);

            _effectCoroutine = StartCoroutine(OverdriveDeactivationSequence());
        }

        /// <summary>
        /// 오버드라이브 진입 시퀀스
        /// 1. 펀치 효과 (Chromatic + Lens Distortion)
        /// 2. 지속 효과로 전환
        /// </summary>
        private IEnumerator OverdriveActivationSequence()
        {
            // === Phase 1: Entry Punch ===
            float halfPunch = _punchDuration / 2f;

            // Punch In (0 -> Peak)
            float elapsed = 0f;
            while (elapsed < halfPunch)
            {
                float t = elapsed / halfPunch;
                t = EaseOutQuad(t);

                _chromaticAberration.intensity.Override(Mathf.Lerp(0f, _punchChromaticPeak, t));
                _lensDistortion.intensity.Override(Mathf.Lerp(0f, _punchLensDistortionPeak, t));

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            // Punch Out (Peak -> Sustained)
            elapsed = 0f;
            while (elapsed < halfPunch)
            {
                float t = elapsed / halfPunch;
                t = EaseInQuad(t);

                _chromaticAberration.intensity.Override(Mathf.Lerp(_punchChromaticPeak, _sustainedChromaticIntensity, t));
                _lensDistortion.intensity.Override(Mathf.Lerp(_punchLensDistortionPeak, 0f, t));

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            _lensDistortion.intensity.Override(0f);
            _chromaticAberration.intensity.Override(_sustainedChromaticIntensity);

            // === Phase 2: Fade In Sustained Effects ===
            elapsed = 0f;
            while (elapsed < _fadeInDuration)
            {
                float t = elapsed / _fadeInDuration;
                t = EaseOutQuad(t);

                // Bloom
                _bloom.intensity.Override(Mathf.Lerp(_originalBloomIntensity, _originalBloomIntensity + _bloomIntensityIncrease, t));

                // Vignette
                _vignette.intensity.Override(Mathf.Lerp(_originalVignetteIntensity, _vignetteIntensity, t));
                _vignette.color.Override(Color.Lerp(_originalVignetteColor, _vignetteColor, t));

                // Color Adjustments
                _colorAdjustments.colorFilter.Override(Color.Lerp(_originalColorFilter, _colorFilter, t));
                _colorAdjustments.saturation.Override(Mathf.Lerp(_originalSaturation, _originalSaturation + _saturationIncrease, t));
                _colorAdjustments.contrast.Override(Mathf.Lerp(_originalContrast, _originalContrast + _contrastIncrease, t));

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            // Final values
            _bloom.intensity.Override(_originalBloomIntensity + _bloomIntensityIncrease);
            _vignette.intensity.Override(_vignetteIntensity);
            _vignette.color.Override(_vignetteColor);
            _colorAdjustments.colorFilter.Override(_colorFilter);
            _colorAdjustments.saturation.Override(_originalSaturation + _saturationIncrease);
            _colorAdjustments.contrast.Override(_originalContrast + _contrastIncrease);

            _effectCoroutine = null;
        }

        /// <summary>
        /// 오버드라이브 종료 시퀀스
        /// </summary>
        private IEnumerator OverdriveDeactivationSequence()
        {
            // Store current values for smooth transition
            float currentBloom = _bloom.intensity.value;
            float currentVignetteIntensity = _vignette.intensity.value;
            Color currentVignetteColor = _vignette.color.value;
            Color currentColorFilter = _colorAdjustments.colorFilter.value;
            float currentSaturation = _colorAdjustments.saturation.value;
            float currentContrast = _colorAdjustments.contrast.value;
            float currentChromatic = _chromaticAberration.intensity.value;

            float elapsed = 0f;
            while (elapsed < _fadeOutDuration)
            {
                float t = elapsed / _fadeOutDuration;
                t = EaseInOutQuad(t);

                _bloom.intensity.Override(Mathf.Lerp(currentBloom, _originalBloomIntensity, t));
                _vignette.intensity.Override(Mathf.Lerp(currentVignetteIntensity, _originalVignetteIntensity, t));
                _vignette.color.Override(Color.Lerp(currentVignetteColor, _originalVignetteColor, t));
                _colorAdjustments.colorFilter.Override(Color.Lerp(currentColorFilter, _originalColorFilter, t));
                _colorAdjustments.saturation.Override(Mathf.Lerp(currentSaturation, _originalSaturation, t));
                _colorAdjustments.contrast.Override(Mathf.Lerp(currentContrast, _originalContrast, t));
                _chromaticAberration.intensity.Override(Mathf.Lerp(currentChromatic, _originalChromaticIntensity, t));

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            // Reset to original
            _bloom.intensity.Override(_originalBloomIntensity);
            _vignette.intensity.Override(_originalVignetteIntensity);
            _vignette.color.Override(_originalVignetteColor);
            _colorAdjustments.colorFilter.Override(_originalColorFilter);
            _colorAdjustments.saturation.Override(_originalSaturation);
            _colorAdjustments.contrast.Override(_originalContrast);
            _chromaticAberration.intensity.Override(_originalChromaticIntensity);
            _lensDistortion.intensity.Override(0f);

            _effectCoroutine = null;
        }

        #region Easing Functions

        private float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
        private float EaseInQuad(float t) => t * t;
        private float EaseInOutQuad(float t) => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;

        #endregion

        private void OnDestroy()
        {
            // Reset to original values on destroy
            if (_isInitialized)
            {
                _bloom?.intensity.Override(_originalBloomIntensity);
                _vignette?.intensity.Override(_originalVignetteIntensity);
                _vignette?.color.Override(_originalVignetteColor);
                _colorAdjustments?.colorFilter.Override(_originalColorFilter);
                _colorAdjustments?.saturation.Override(_originalSaturation);
                _colorAdjustments?.contrast.Override(_originalContrast);
                _chromaticAberration?.intensity.Override(_originalChromaticIntensity);
                _lensDistortion?.intensity.Override(0f);
            }
        }
    }
}