using System;
using UnityEngine;
using _02.Scripts.Common;
using _02.Scripts.Interfaces;
using _02.Scripts.Player.Data;

namespace _02.Scripts.Player.Combat
{
    // 크레센트 원거리 스킬 (오브젝트 풀링 적용)
    public class CrescentSkill : MonoBehaviour, ISkill
    {
        private const string CooldownKey = "Crescent";
        private const int PoolInitialSize = 5;

        [Header("References")]
        [SerializeField] private CrescentProjectile _projectilePrefab;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private Transform _poolContainer;

        private PlayerStats _stats;
        private CooldownManager _cooldownManager;
        private ObjectPool<CrescentProjectile> _projectilePool;

        // ISkill
        public string SkillName => "크레센트";
        public float Cooldown => _stats != null ? _stats.CrescentCooldown : 1f;
        public bool CanUse => _cooldownManager.IsReady(CooldownKey, Cooldown);

        private float Damage => _stats != null ? _stats.CrescentDamage : 15f;
        private float Speed => _stats != null ? _stats.CrescentSpeed : 20f;
        private float Range => _stats != null ? _stats.CrescentRange : 30f;

        public event Action OnSkillUsed;
        public event Action<IDamageable, float> OnEnemyHit;

        private void Awake()
        {
            _stats = GetComponent<PlayerStats>();
            _cooldownManager = new CooldownManager();

            if (_projectilePrefab != null)
            {
                _projectilePool = new ObjectPool<CrescentProjectile>(_projectilePrefab, _poolContainer, PoolInitialSize);
            }

            if (_firePoint == null) _firePoint = transform;
            if (_cameraTransform == null) _cameraTransform = Camera.main?.transform;
        }

        // 외부 Stats 주입
        public void Initialize(PlayerStats stats) => _stats = stats;

        public void Use() => Fire();

        // 카메라 방향으로 프로젝타일 발사
        public void Fire()
        {
            if (!CanUse || _projectilePool == null) return;

            _cooldownManager.Use(CooldownKey);

            Vector3 direction = _cameraTransform.forward;
            CrescentProjectile projectile = _projectilePool.Get();
            projectile.transform.position = _firePoint.position;
            projectile.Initialize(Damage, Speed, Range, direction, gameObject, ReturnProjectile);
            projectile.OnHit += HandleProjectileHit;

            OnSkillUsed?.Invoke();
        }

        // 풀 반환
        private void ReturnProjectile(CrescentProjectile projectile)
        {
            projectile.OnHit -= HandleProjectileHit;
            _projectilePool.Return(projectile);
        }

        private void HandleProjectileHit(IDamageable target, float damage) => OnEnemyHit?.Invoke(target, damage);

        private void OnDestroy() => _projectilePool?.Clear();
    }
}