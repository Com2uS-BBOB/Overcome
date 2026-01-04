using System;
using UnityEngine;
using _02.Scripts.Player.Data;

namespace _02.Scripts.Player.Combat
{
    /// <summary>
    /// 크레센트 스킬 관리
    /// 프로젝타일 발사 및 쿨타임 처리
    /// </summary>
    public class CrescentSkill : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private Transform _firePoint;

        private PlayerRuntimeStats _stats;
        private float _lastFireTime = -100f;

        // 스탯 참조
        private float Damage => _stats != null ? _stats.CrescentDamage : 15f;
        private float Speed => _stats != null ? _stats.CrescentSpeed : 20f;
        private float Range => _stats != null ? _stats.CrescentRange : 30f;
        private float Cooldown => _stats != null ? _stats.CrescentCooldown : 1f;

        /// <summary>
        /// 발사 가능 여부
        /// </summary>
        public bool CanFire => Time.time >= _lastFireTime + Cooldown;

        // 이벤트
        public event Action OnFired;
        public event Action<Collider, float> OnEnemyHit;

        private void Awake()
        {
            _stats = GetComponent<PlayerRuntimeStats>();

            if (_stats == null)
            {
                Debug.LogWarning("[CrescentSkill] RuntimeStats 없음 - 기본값 사용");
            }

            if (_projectilePrefab == null)
            {
                Debug.LogError("[CrescentSkill] Projectile Prefab이 할당되지 않았습니다!");
            }

            // FirePoint 기본 생성
            if (_firePoint == null)
            {
                _firePoint = transform;
            }
        }

        /// <summary>
        /// 외부에서 Stats 주입
        /// </summary>
        public void Initialize(PlayerRuntimeStats stats)
        {
            _stats = stats;
        }

        /// <summary>
        /// 크레센트 발사
        /// </summary>
        public void Fire()
        {
            if (!CanFire)
            {
                float remaining = (_lastFireTime + Cooldown) - Time.time;
                Debug.Log($"[Crescent] 발사 불가 - 쿨타임 {remaining:F1}초 남음");
                return;
            }

            if (_projectilePrefab == null)
            {
                Debug.LogError("[Crescent] Projectile Prefab 없음!");
                return;
            }

            _lastFireTime = Time.time;

            // 발사 위치 및 방향
            Vector3 spawnPosition = _firePoint.position;
            Vector3 direction = transform.forward;

            // 프로젝타일 생성
            GameObject projectileObj = Instantiate(_projectilePrefab, spawnPosition, Quaternion.identity);
            CrescentProjectile projectile = projectileObj.GetComponent<CrescentProjectile>();

            if (projectile != null)
            {
                projectile.Initialize(Damage, Speed, Range, direction);
                projectile.OnHit += HandleProjectileHit;
            }

            Debug.Log("[Crescent] 크레센트 발사!");
            OnFired?.Invoke();
        }

        private void HandleProjectileHit(Collider target, float damage)
        {
            OnEnemyHit?.Invoke(target, damage);
        }
    }
}