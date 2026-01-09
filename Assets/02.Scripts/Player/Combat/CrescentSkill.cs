using System;
using System.Collections;
using UnityEngine;
using _02.Scripts.Player.Common;
using _02.Scripts.Player.Interfaces;
using _02.Scripts.Player.Data;
using _02.Scripts.Player.Gauge;

namespace _02.Scripts.Player.Combat
{
    // 크레센트 원거리 스킬 (게이지 기반)
    public class CrescentSkill : MonoBehaviour, ISkill, IOverDriveAffected
    {
        private const int PoolInitialSize = 5;

        [Header("Duration Settings")]
        [SerializeField] private float _skillDuration = 0.6f;
        [SerializeField] private float _comboWindow = 0.8f;  // 콤보 입력 허용 시간

        [Header("References")]
        [SerializeField] private CrescentProjectile _projectilePrefab;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private Transform _poolContainer;

        private PlayerStats _stats;
        private GaugeManager _gaugeManager;
        private ObjectPool<CrescentProjectile> _projectilePool;

        // 상태
        private bool _isUsing;
        private Coroutine _skillCoroutine;

        // 콤보 시스템
        private int _comboStep;
        private float _comboTimer;
        private const int MaxCombo = 2;

        // ISkill
        public string SkillName => "크레센트";
        public float Cooldown => 0f;
        public bool CanUse => !_isUsing && _gaugeManager != null && _gaugeManager.CanUseCrescent;
        public bool IsUsing => _isUsing;

        // IOverDriveAffected
        public bool IsOverDriveActive { get; set; }

        private float Damage => _stats != null ? _stats.CrescentDamage : 15f;
        private float Speed => _stats != null ? _stats.CrescentSpeed : 20f;
        private float Range => _stats != null ? _stats.CrescentRange : 30f;

        // 콤보 프로퍼티
        public int ComboStep => _comboStep;
        public bool CanCombo => _comboStep > 0 && _comboStep < MaxCombo && _comboTimer > 0f;

        public event Action OnSkillUsed;
        public event Action OnCrescentEnded;
        public event Action<IDamageable, float> OnEnemyHit;
        public event Action<int> OnComboAttack;  // comboStep 전달

        private void Awake()
        {
            if (_projectilePrefab != null)
            {
                _projectilePool = new ObjectPool<CrescentProjectile>(_projectilePrefab, _poolContainer, PoolInitialSize);
            }

            if (_firePoint == null) _firePoint = transform;
            if (_cameraTransform == null) _cameraTransform = Camera.main?.transform;
        }

        // 외부 Stats 및 GaugeManager 주입
        public void Initialize(PlayerStats stats, GaugeManager gaugeManager)
        {
            _stats = stats;
            _gaugeManager = gaugeManager;
        }

        private void Update()
        {
            // 콤보 타이머 감소
            if (_comboTimer > 0f)
            {
                _comboTimer -= Time.deltaTime;
                if (_comboTimer <= 0f)
                    ResetCombo();
            }
        }

        /// <summary>
        /// 크레센트 공격 (콤보 시스템)
        /// </summary>
        public void Attack()
        {
            // 콤보 연속 입력
            if (CanCombo && _gaugeManager.CanUseCrescent)
            {
                _comboStep++;
                _comboTimer = _comboWindow;
                FireProjectile();
                OnComboAttack?.Invoke(_comboStep);
                return;
            }

            // 첫 공격
            if (CanUse)
            {
                _comboStep = 1;
                _comboTimer = _comboWindow;
                FireProjectile();
                OnComboAttack?.Invoke(_comboStep);
            }
        }

        public void Use() => Attack();

        /// <summary>
        /// 프로젝타일 발사 (내부용)
        /// </summary>
        private void FireProjectile()
        {
            if (_projectilePool == null) return;

            // 오버드라이브 중이 아닐 때만 게이지 소모
            if (!IsOverDriveActive)
                _gaugeManager.ConsumeCrescent();

            Vector3 direction = _cameraTransform.forward;
            CrescentProjectile projectile = _projectilePool.Get();
            projectile.transform.position = _firePoint.position;
            projectile.Initialize(Damage, Speed, Range, direction, gameObject, ReturnProjectile);
            projectile.OnHit += HandleProjectileHit;

            // 스킬 지속 시간 코루틴 시작
            if (_skillCoroutine != null)
                StopCoroutine(_skillCoroutine);
            _skillCoroutine = StartCoroutine(SkillDurationCoroutine());

            OnSkillUsed?.Invoke();
        }

        /// <summary>
        /// 콤보 리셋
        /// </summary>
        public void ResetCombo()
        {
            _comboStep = 0;
            _comboTimer = 0f;
        }

        private IEnumerator SkillDurationCoroutine()
        {
            _isUsing = true;
            yield return new WaitForSeconds(_skillDuration);
            _isUsing = false;
            _skillCoroutine = null;
            OnCrescentEnded?.Invoke();
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