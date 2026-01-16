using System;
using UnityEngine;
using _02.Scripts.Player.Common;
using _02.Scripts.Player.Interfaces;
using _02.Scripts.Player.Data;
using _02.Scripts.Player.Gauge;

namespace _02.Scripts.Player.Combat
{
    /// <summary>
    /// 크레센트 원거리 스킬 (게이지 기반)
    /// 지상 콤보와 공중 콤보가 SkillDataSet으로 분리되어 관리됨
    /// </summary>
    public class CrescentSkill : BaseSkill, IOverDriveAffected
    {
        private const int PoolInitialSize = 10;

        [Header("Projectile")]
        [SerializeField] private CrescentProjectile _projectilePrefab;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private Transform _poolContainer;

        private PlayerStats _stats;
        private GaugeManager _gaugeManager;
        private ObjectPool<CrescentProjectile> _projectilePool;

        // === ISkill 구현 ===
        public override string SkillName => "크레센트";
        public override float Cooldown => 0f;
        public override bool CanUse => !_isActive && !_inComboGrace &&
                                       _gaugeManager != null && _gaugeManager.CanUseCrescent;

        // === IOverDriveAffected 구현 ===
        public bool IsOverDriveActive { get; set; }

        // === Properties ===

        /// <summary>
        /// 스킬 사용 중 여부 (State 체크용)
        /// </summary>
        public bool IsUsing => _isActive;

        private float Damage => _stats != null ? _stats.CrescentDamage : 15f;
        private float Speed => _stats != null ? _stats.CrescentSpeed : 20f;
        private float Range => _stats != null ? _stats.CrescentRange : 30f;

        // === Events ===
        public event Action OnCrescentEnded;

        // === Initialization ===

        private void Awake()
        {
            if (_projectilePrefab != null)
            {
                _projectilePool = new ObjectPool<CrescentProjectile>(
                    _projectilePrefab, _poolContainer, PoolInitialSize);
            }

            if (_firePoint == null) _firePoint = transform;
            if (_cameraTransform == null) _cameraTransform = Camera.main?.transform;
        }

        private void OnEnable()
        {
            OnSkillEnded += HandleSkillEnded;
        }

        private void OnDisable()
        {
            OnSkillEnded -= HandleSkillEnded;
        }

        private void HandleSkillEnded()
        {
            OnCrescentEnded?.Invoke();
        }

        public void Initialize(PlayerStats stats, GaugeManager gaugeManager)
        {
            _stats = stats;
            _gaugeManager = gaugeManager;
        }

        // === BaseSkill Override ===

        protected override bool GetIsGrounded()
        {
            // 크레센트는 지상/공중 구분을 위해 별도 Movement 참조 필요
            // 현재는 true 반환 (PlayerController에서 StartSkill 호출 시 isGrounded 전달)
            return true;
        }

        protected override float GetDamage()
        {
            return Damage;
        }

        protected override void OnSkillStart()
        {
            // 애니메이션 이벤트에서 발사하도록 변경
            // FireProjectile()은 FireFromAnimationEvent()에서 호출됨
        }

        /// <summary>
        /// 애니메이션 이벤트에서 호출되는 발사 메서드
        /// </summary>
        public void FireFromAnimationEvent()
        {
            FireProjectile();
        }

        // 크레센트는 히트박스 대신 프로젝타일 사용
        protected override void EnableHitbox() { }
        protected override void DisableHitbox() { }

        // === Public API (호환성 유지) ===

        /// <summary>
        /// 첫 크레센트 공격 (콤보 1타)
        /// </summary>
        public void Attack()
        {
            StartSkill(GetIsGrounded());
        }

        /// <summary>
        /// 콤보 큐잉 Override (게이지 체크 추가)
        /// </summary>
        public override void QueueCombo()
        {
            // 게이지 검증 (오버드라이브 시에는 무시)
            bool hasGauge = IsOverDriveActive || _gaugeManager.CanUseCrescent;

            if (_isActive && !_comboQueued && CanContinueCombo && hasGauge)
            {
                _comboQueued = true;
                // OnComboQueued 이벤트는 BaseSkill에서 호출됨
            }
        }

        /// <summary>
        /// 콤보 유예 중 다음 콤보 실행 Override (게이지 체크 추가)
        /// </summary>
        public override void ExecuteGraceCombo()
        {
            // 게이지 검증 (오버드라이브 시에는 무시)
            bool hasGauge = IsOverDriveActive || _gaugeManager.CanUseCrescent;

            if (CanComboGrace && hasGauge)
            {
                StopGraceTimer();
                ExecuteNextCombo();
            }
        }

        // === Private Methods ===

        private void FireProjectile()
        {
            if (_projectilePool == null) return;

            // 오버드라이브 중이 아닐 때만 게이지 소모
            if (!IsOverDriveActive)
                _gaugeManager.ConsumeCrescent();

            Vector3 direction = _cameraTransform != null ? _cameraTransform.forward : transform.forward;
            CrescentProjectile projectile = _projectilePool.Get();
            projectile.transform.position = _firePoint.position;
            direction.y += 0.2f;
            direction.Normalize();
            projectile.Initialize(Damage, Speed, Range, direction, gameObject, ReturnProjectile);
            projectile.OnHit += HandleProjectileHit;
        }

        private void ReturnProjectile(CrescentProjectile projectile)
        {
            projectile.OnHit -= HandleProjectileHit;
            _projectilePool.Return(projectile);
        }

        private void HandleProjectileHit(IDamageable target, float damage)
        {
            InvokeEnemyHit(target, damage);
        }

        private void OnDestroy()
        {
            _projectilePool?.Clear();
        }
    }
}