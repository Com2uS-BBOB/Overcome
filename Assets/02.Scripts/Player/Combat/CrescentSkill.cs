using System;
using System.Collections;
using UnityEngine;
using _02.Scripts.Player.Common;
using _02.Scripts.Player.Interfaces;
using _02.Scripts.Player.Data;
using _02.Scripts.Player.Gauge;

namespace _02.Scripts.Player.Combat
{
    // 크레센트 원거리 스킬 (게이지 기반) - 2타 콤보
    // 콤보 윈도우 + 콤보 유예 시스템
    public class CrescentSkill : MonoBehaviour, ISkill, IOverDriveAffected
    {
        private const int PoolInitialSize = 5;
        private const int MaxCombo = 2;

        [Header("Duration Settings")]
        [SerializeField] private float _skill1Duration = 0.6f;
        [SerializeField] private float _skill2Duration = 0.7f;

        [Header("Combo Settings")]
        [SerializeField] private float _comboWindowStart = 0.25f;  // 콤보 윈도우 시작 시점
        [SerializeField] private float _comboGraceTime = 0.1f;     // 스킬 종료 후 유예 시간

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
        private int _comboStep;
        private bool _comboQueued;
        private bool _inComboWindow;
        private bool _inComboGrace;
        private bool _skipToNextCombo;
        private float _skillElapsedTime;
        private float _currentSkillDuration;

        private Coroutine _skillCoroutine;
        private Coroutine _comboGraceCoroutine;

        // ISkill
        public string SkillName => "크레센트";
        public float Cooldown => 0f;
        public bool CanUse => !_isUsing && !_inComboGrace && _gaugeManager != null && _gaugeManager.CanUseCrescent;
        public bool IsUsing => _isUsing;
        public int ComboStep => _comboStep;

        // IOverDriveAffected
        public bool IsOverDriveActive { get; set; }

        // 콤보 큐잉 가능: 스킬 사용 중 + 콤보 윈도우 내 + 아직 예약 안됨 + 마지막 콤보 아님
        public bool CanQueueCombo => _isUsing && _inComboWindow && !_comboQueued && _comboStep < MaxCombo;

        // 콤보 유예 가능: 유예 구간 내 + 다음 콤보 있음
        public bool CanComboGrace => _inComboGrace && _comboStep < MaxCombo;

        private float Damage => _stats != null ? _stats.CrescentDamage : 15f;
        private float Speed => _stats != null ? _stats.CrescentSpeed : 20f;
        private float Range => _stats != null ? _stats.CrescentRange : 30f;
        private float GetSkillDuration(int step) => step == 1 ? _skill1Duration : _skill2Duration;

        public event Action OnSkillUsed;
        public event Action OnCrescentEnded;
        public event Action<IDamageable, float> OnEnemyHit;
        public event Action<int> OnComboAttack;

        private void Awake()
        {
            if (_projectilePrefab != null)
            {
                _projectilePool = new ObjectPool<CrescentProjectile>(_projectilePrefab, _poolContainer, PoolInitialSize);
            }

            if (_firePoint == null) _firePoint = transform;
            if (_cameraTransform == null) _cameraTransform = Camera.main?.transform;
        }

        public void Initialize(PlayerStats stats, GaugeManager gaugeManager)
        {
            _stats = stats;
            _gaugeManager = gaugeManager;
        }

        public void Use() => Attack();

        /// <summary>
        /// 크레센트 공격 - 상황에 따라 첫 공격/콤보 큐잉/콤보 유예 처리
        /// </summary>
        public void Attack()
        {
            // Case 1: 콤보 큐잉 (1타 진행 중 입력) → 즉시 스킵
            if (CanQueueCombo && _gaugeManager.CanUseCrescent)
            {
                _comboQueued = true;
                _skipToNextCombo = true;
                return;
            }

            // Case 2: 콤보 유예 (1타 끝난 직후 입력)
            if (CanComboGrace && _gaugeManager.CanUseCrescent)
            {
                StopGraceTimer();
                ExecuteNextCombo();
                return;
            }

            // Case 3: 첫 공격
            if (CanUse)
            {
                StopAllTimers();
                _comboStep = 1;
                _skillCoroutine = StartCoroutine(SkillCoroutine());
            }
        }

        private IEnumerator SkillCoroutine()
        {
            _isUsing = true;
            _comboQueued = false;
            _inComboWindow = false;
            _inComboGrace = false;
            _skipToNextCombo = false;

            _currentSkillDuration = GetSkillDuration(_comboStep);
            _skillElapsedTime = 0f;

            // 게이지 소모 및 프로젝타일 발사
            FireProjectile();

            OnSkillUsed?.Invoke();
            OnComboAttack?.Invoke(_comboStep);

            // 매 프레임 경과 시간 추적 + 스킵 체크
            while (_skillElapsedTime < _currentSkillDuration)
            {
                _skillElapsedTime += Time.deltaTime;

                // 콤보 윈도우 시작
                if (!_inComboWindow && _skillElapsedTime >= _comboWindowStart && _comboStep < MaxCombo)
                {
                    _inComboWindow = true;
                }

                // 스킵 체크: 콤보가 큐잉되고 스킵 플래그가 설정되면 즉시 종료
                if (_skipToNextCombo && _comboQueued)
                {
                    break;
                }

                yield return null;
            }

            // 스킬 종료
            _isUsing = false;
            _inComboWindow = false;
            _skipToNextCombo = false;

            // 콤보 처리
            if (_comboQueued && _comboStep < MaxCombo)
            {
                ExecuteNextCombo();
            }
            else if (_comboStep < MaxCombo)
            {
                StartGraceTimer();
            }
            else
            {
                // 마지막 콤보 완료
                _comboStep = 0;
                OnCrescentEnded?.Invoke();
            }
        }

        private void ExecuteNextCombo()
        {
            _comboStep++;
            _comboQueued = false;
            _skillCoroutine = StartCoroutine(SkillCoroutine());
        }

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
        }

        private void StartGraceTimer()
        {
            _inComboGrace = true;
            _comboGraceCoroutine = StartCoroutine(GraceTimerCoroutine());
        }

        private IEnumerator GraceTimerCoroutine()
        {
            yield return new WaitForSeconds(_comboGraceTime);

            // 유예 시간 종료
            _inComboGrace = false;
            _comboStep = 0;
            OnCrescentEnded?.Invoke();
        }

        private void StopGraceTimer()
        {
            _inComboGrace = false;
            if (_comboGraceCoroutine != null)
            {
                StopCoroutine(_comboGraceCoroutine);
                _comboGraceCoroutine = null;
            }
        }

        private void StopAllTimers()
        {
            StopGraceTimer();
            if (_skillCoroutine != null)
            {
                StopCoroutine(_skillCoroutine);
                _skillCoroutine = null;
            }
        }

        public void ResetCombo()
        {
            StopAllTimers();
            _comboStep = 0;
            _isUsing = false;
            _comboQueued = false;
            _inComboWindow = false;
            _inComboGrace = false;
        }

        private void ReturnProjectile(CrescentProjectile projectile)
        {
            projectile.OnHit -= HandleProjectileHit;
            _projectilePool.Return(projectile);
        }

        private void HandleProjectileHit(IDamageable target, float damage) => OnEnemyHit?.Invoke(target, damage);

        private void OnDestroy() => _projectilePool?.Clear();
    }
}