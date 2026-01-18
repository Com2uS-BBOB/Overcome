using UnityEngine;

namespace _02.Scripts.Player.Overdrive
{
    /// <summary>
    /// Overdrive VFX 설정값을 관리하는 ScriptableObject
    /// 스텔라 블레이드 타키모드 스타일 참조
    /// </summary>
    [CreateAssetMenu(fileName = "OverdriveVFXSettings", menuName = "Overcome/Overdrive VFX Settings")]
    public class OverdriveVFXSettings : ScriptableObject
    {
        [Header("=== 진입 연출 (Activation) ===")]
        [Tooltip("진입 슬로우모션 지속 시간")]
        public float ActivationSlowDuration = 0.4f;

        [Tooltip("슬로우모션 시 TimeScale (0.1 = 10% 속도)")]
        [Range(0.01f, 0.5f)]
        public float ActivationTimeScale = 0.1f;

        [Tooltip("슬로우모션 복귀 시간")]
        public float TimeScaleRecoveryDuration = 0.1f;

        [Tooltip("진입 시 카메라 줌아웃 FOV 증가량")]
        public float ActivationFOVIncrease = 12f;

        [Tooltip("진입 시 FOV 전환 시간")]
        public float ActivationFOVTransitionTime = 0.2f;

        [Tooltip("진입 시 카메라 셰이크 강도")]
        public float ActivationShakeIntensity = 0.6f;

        [Tooltip("진입 플래시 지속 시간")]
        public float FlashDuration = 0.15f;

        [Tooltip("진입 플래시 색상")]
        public Color FlashColor = new Color(0.5f, 0.8f, 1f, 0.8f);

        [Header("=== Trail 색상 설정 ===")]
        [Tooltip("Overdrive Trail 메인 색상 (HDR)")]
        [ColorUsage(true, true)]
        public Color TrailColorMain = new Color(0f, 2f, 3f, 1f);

        [Tooltip("Overdrive Trail 보조 색상 (HDR)")]
        [ColorUsage(true, true)]
        public Color TrailColorSecondary = new Color(0.5f, 0f, 3f, 1f);

        [Header("=== 무기 발광 설정 ===")]
        [Tooltip("무기 Emission 색상 (HDR)")]
        [ColorUsage(true, true)]
        public Color WeaponEmissionColor = new Color(0f, 2f, 4f, 1f);

        [Tooltip("무기 Emission 강도 배율")]
        [Range(1f, 10f)]
        public float WeaponEmissionMultiplier = 3f;

        [Header("=== Post Processing 설정 ===")]
        [Tooltip("Bloom 강도 증가량")]
        public float BloomIntensityBoost = 0.5f;

        [Tooltip("Chromatic Aberration 강도")]
        [Range(0f, 1f)]
        public float ChromaticAberration = 0.15f;

        [Tooltip("Vignette 강도")]
        [Range(0f, 0.5f)]
        public float VignetteIntensity = 0.25f;

        [Tooltip("Vignette 색상")]
        public Color VignetteColor = new Color(0f, 0.3f, 0.5f, 1f);

        [Tooltip("Color Grading - 채도 증가")]
        [Range(-50f, 50f)]
        public float SaturationBoost = 15f;

        [Tooltip("Color Grading - 대비 증가")]
        [Range(-50f, 50f)]
        public float ContrastBoost = 10f;

        [Header("=== 지속 중 효과 ===")]
        [Tooltip("지속 중 화면 펄스 주기")]
        public float PulsePeriod = 2f;

        [Tooltip("지속 중 화면 펄스 강도")]
        [Range(0f, 0.3f)]
        public float PulseIntensity = 0.1f;

        [Header("=== 종료 연출 (Deactivation) ===")]
        [Tooltip("종료 페이드아웃 지속 시간")]
        public float DeactivationFadeDuration = 0.5f;

        [Tooltip("종료 시 짧은 슬로우모션 지속 시간")]
        public float DeactivationSlowDuration = 0.2f;

        [Tooltip("종료 슬로우모션 TimeScale")]
        [Range(0.3f, 0.8f)]
        public float DeactivationTimeScale = 0.5f;

        [Tooltip("종료 시 카메라 셰이크 강도")]
        public float DeactivationShakeIntensity = 0.3f;

        [Header("=== 트랜지션 설정 ===")]
        [Tooltip("효과 전환 Easing 시간")]
        public float TransitionDuration = 0.3f;
    }
}