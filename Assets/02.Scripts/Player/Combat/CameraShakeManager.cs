using UnityEngine;
using Unity.Cinemachine;
using _02.Scripts.Player.Data;

namespace _02.Scripts.Player.Combat
{
    /// <summary>
    /// 카메라 쉐이크 관리 (Cinemachine Impulse 기반)
    /// </summary>
    public class CameraShakeManager : MonoBehaviour
    {
        public static CameraShakeManager Instance { get; private set; }

        [Header("Impulse Source")]
        [SerializeField] private CinemachineImpulseSource _impulseSource;

        [Header("Impulse Settings")]
        [SerializeField] private float _justGuardIntensity = 0.3f;
        [SerializeField] private float _normalGuardIntensity = 0.15f;
        [SerializeField] private float _guardBreakIntensity = 0.5f;
        [SerializeField] private float _hitLightIntensity = 0.2f;
        [SerializeField] private float _hitHeavyIntensity = 0.4f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (_impulseSource == null)
            {
                _impulseSource = GetComponent<CinemachineImpulseSource>();
            }
        }

        /// <summary>
        /// 저스트 가드 성공 시 카메라 쉐이크
        /// </summary>
        public void OnJustGuard()
        {
            GenerateImpulse(_justGuardIntensity);
        }

        /// <summary>
        /// 일반 가드 성공 시 카메라 쉐이크
        /// </summary>
        public void OnNormalGuard()
        {
            GenerateImpulse(_normalGuardIntensity);
        }

        /// <summary>
        /// 가드 브레이크 시 카메라 쉐이크
        /// </summary>
        public void OnGuardBreak()
        {
            GenerateImpulse(_guardBreakIntensity);
        }

        /// <summary>
        /// 피격 시 카메라 쉐이크
        /// </summary>
        public void OnHit(KnockbackLevel level)
        {
            float intensity = level == KnockbackLevel.Heavy ? _hitHeavyIntensity : _hitLightIntensity;
            GenerateImpulse(intensity);
        }

        /// <summary>
        /// 커스텀 강도로 쉐이크
        /// </summary>
        public void Shake(float intensity)
        {
            GenerateImpulse(intensity);
        }

        private void GenerateImpulse(float intensity)
        {
            if (_impulseSource != null)
            {
                _impulseSource.GenerateImpulse(intensity);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}