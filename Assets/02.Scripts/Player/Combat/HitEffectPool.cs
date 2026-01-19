using System.Collections;
using UnityEngine;
using _02.Scripts.Player.Common;

namespace _02.Scripts.Player.Combat
{
    /// <summary>
    /// 히트 이펙트 풀링 관리자
    /// 타격 시 이펙트를 풀에서 가져와 스폰하고 재생 완료 후 반환
    /// Overdrive 상태에 따라 다른 이펙트 사용
    /// </summary>
    public class HitEffectPool : MonoBehaviour
    {
        public static HitEffectPool Instance { get; private set; }

        [Header("Normal Settings")]
        [SerializeField] private ParticleSystem _hitEffectPrefab;
        [SerializeField] private int _initialPoolSize = 10;

        [Header("Overdrive Settings")]
        [SerializeField] private ParticleSystem _overdriveHitEffectPrefab;

        private ObjectPool<ParticleSystem> _normalPool;
        private ObjectPool<ParticleSystem> _overdrivePool;
        private bool _isOverdriveActive;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializePools();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializePools()
        {
            // Normal Pool
            if (_hitEffectPrefab != null)
            {
                _normalPool = new ObjectPool<ParticleSystem>(_hitEffectPrefab, transform, _initialPoolSize);
            }
            else
            {
                Debug.LogWarning("[HitEffectPool] Normal hit effect prefab is not assigned.");
            }

            // Overdrive Pool
            if (_overdriveHitEffectPrefab != null)
            {
                _overdrivePool = new ObjectPool<ParticleSystem>(_overdriveHitEffectPrefab, transform, _initialPoolSize);
            }
        }

        /// <summary>
        /// Overdrive 상태 설정 (OverdriveVFXController에서 호출)
        /// </summary>
        public void SetOverdriveActive(bool active)
        {
            _isOverdriveActive = active;
        }

        /// <summary>
        /// 지정 위치에 히트 이펙트 스폰
        /// </summary>
        public void SpawnAt(Vector3 position)
        {
            SpawnAt(position, Quaternion.identity);
        }

        /// <summary>
        /// 지정 위치와 회전으로 히트 이펙트 스폰
        /// </summary>
        public void SpawnAt(Vector3 position, Quaternion rotation)
        {
            // Overdrive 상태에 따라 풀 선택
            var pool = GetActivePool();
            if (pool == null) return;

            var effect = pool.Get();
            if (effect == null) return;

            effect.transform.SetPositionAndRotation(position, rotation);
            effect.Clear();  // 이전 파티클 정리
            effect.Play();

            StartCoroutine(ReturnAfterPlay(effect, pool));
        }

        private ObjectPool<ParticleSystem> GetActivePool()
        {
            // Overdrive 상태이고 Overdrive 풀이 있으면 사용
            if (_isOverdriveActive && _overdrivePool != null)
                return _overdrivePool;

            return _normalPool;
        }

        private IEnumerator ReturnAfterPlay(ParticleSystem effect, ObjectPool<ParticleSystem> pool)
        {
            // 파티클 재생 시간 + 수명 만큼 대기
            var main = effect.main;
            float duration = main.duration + main.startLifetime.constantMax;
            yield return new WaitForSeconds(duration);

            effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            pool.Return(effect);
        }

        private void OnDestroy()
        {
            _normalPool?.Clear();
            _overdrivePool?.Clear();
        }
    }
}