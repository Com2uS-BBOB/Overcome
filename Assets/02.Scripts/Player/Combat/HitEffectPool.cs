using System.Collections;
using UnityEngine;
using _02.Scripts.Player.Common;

namespace _02.Scripts.Player.Combat
{
    /// <summary>
    /// 히트 이펙트 풀링 관리자
    /// 타격 시 이펙트를 풀에서 가져와 스폰하고 재생 완료 후 반환
    /// </summary>
    public class HitEffectPool : MonoBehaviour
    {
        public static HitEffectPool Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private ParticleSystem _hitEffectPrefab;
        [SerializeField] private int _initialPoolSize = 10;

        private ObjectPool<ParticleSystem> _pool;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializePool();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializePool()
        {
            if (_hitEffectPrefab == null)
            {
                Debug.LogWarning("[HitEffectPool] Hit effect prefab is not assigned.");
                return;
            }

            _pool = new ObjectPool<ParticleSystem>(_hitEffectPrefab, transform, _initialPoolSize);
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
            if (_pool == null) return;

            var effect = _pool.Get();
            if (effect == null) return;

            effect.transform.SetPositionAndRotation(position, rotation);
            effect.Clear();  // 이전 파티클 정리
            effect.Play();

            StartCoroutine(ReturnAfterPlay(effect));
        }

        private IEnumerator ReturnAfterPlay(ParticleSystem effect)
        {
            // 파티클 재생 시간 + 수명 만큼 대기
            var main = effect.main;
            float duration = main.duration + main.startLifetime.constantMax;
            yield return new WaitForSeconds(duration);

            effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _pool.Return(effect);
        }

        private void OnDestroy()
        {
            _pool?.Clear();
        }
    }
}