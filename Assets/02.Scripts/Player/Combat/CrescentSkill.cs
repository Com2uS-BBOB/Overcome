using System;
using UnityEngine;
using _02.Scripts.Common;
using _02.Scripts.Interfaces;
using _02.Scripts.Player.Data;

namespace _02.Scripts.Player.Combat
{
    /// <summary>
    /// 크레센트 스킬 관리
    /// ISkill 인터페이스 구현, 오브젝트 풀링 적용
    /// </summary>
    public class CrescentSkill : MonoBehaviour, ISkill
    {
        private const string CooldownKey = "Crescent";
        private const int PoolInitialSize = 5;

        [Header("References")]
        [SerializeField] private CrescentProjectile _projectilePrefab;
        [SerializeField] private Transform _firePoint;

        private PlayerRuntimeStats _stats;
        private CooldownManager _cooldownManager;
        private ObjectPool<CrescentProjectile> _projectilePool;

        // ISkill 구현
        public string SkillName => "크레센트";
        public float Cooldown => _stats != null ? _stats.CrescentCooldown : 1f;
        public bool CanUse => _cooldownManager.IsReady(CooldownKey, Cooldown);

        // 스탯 참조
        private float Damage => _stats != null ? _stats.CrescentDamage : 15f;
        private float Speed => _stats != null ? _stats.CrescentSpeed : 20f;
        private float Range => _stats != null ? _stats.CrescentRange : 30f;

        // 이벤트
        public event Action OnSkillUsed;
        public event Action<IDamageable, float> OnEnemyHit;

        private void Awake()
        {
            _stats = GetComponent<PlayerRuntimeStats>();
            _cooldownManager = new CooldownManager();

            if (_stats == null)
            {
                Debug.LogWarning("[CrescentSkill] RuntimeStats 없음 - 기본값 사용");
            }

            if (_projectilePrefab == null)
            {
                Debug.LogError("[CrescentSkill] Projectile Prefab이 할당되지 않았습니다!");
            }
            else
            {
                _projectilePool = new ObjectPool<CrescentProjectile>(_projectilePrefab, transform, PoolInitialSize);
            }

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
        /// 스킬 사용 (ISkill 구현)
        /// </summary>
        public void Use()
        {
            Fire();
        }

        /// <summary>
        /// 크레센트 발사
        /// </summary>
        public void Fire()
        {
            if (!CanUse)
            {
                float remaining = _cooldownManager.GetRemainingTime(CooldownKey, Cooldown);
                Debug.Log($"[Crescent] 발사 불가 - 쿨타임 {remaining:F1}초 남음");
                return;
            }

            if (_projectilePool == null)
            {
                Debug.LogError("[Crescent] Projectile Pool 없음!");
                return;
            }

            _cooldownManager.Use(CooldownKey);

            // 발사 위치 및 방향
            Vector3 spawnPosition = _firePoint.position;
            Vector3 direction = transform.forward;

            // 풀에서 프로젝타일 가져오기
            CrescentProjectile projectile = _projectilePool.Get();
            projectile.transform.position = spawnPosition;
            projectile.Initialize(Damage, Speed, Range, direction, gameObject, ReturnProjectile);
            projectile.OnHit += HandleProjectileHit;

            Debug.Log("[Crescent] 크레센트 발사!");
            OnSkillUsed?.Invoke();
        }

        /// <summary>
        /// 프로젝타일 풀 반환
        /// </summary>
        private void ReturnProjectile(CrescentProjectile projectile)
        {
            projectile.OnHit -= HandleProjectileHit;
            _projectilePool.Return(projectile);
        }

        private void HandleProjectileHit(IDamageable target, float damage)
        {
            OnEnemyHit?.Invoke(target, damage);
        }

        private void OnDestroy()
        {
            _projectilePool?.Clear();
        }
    }
}