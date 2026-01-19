using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAttack : MonoBehaviour
{
    private Transform _player;
    private EnemyBase _enemy;
    private float _damage;
    private EnemyMovement _movement;
    private EnemyAnimatorController _anim;
    private EnemySlotCoordinator _slotCoordinator;
    private EnemyAttackDirector _attackDirector;
    private NavMeshAgent _agent;
    private EnemyStatData _statData;

    private bool _hasRushedOnce;
    public bool HasRushedOnce => _hasRushedOnce;
    public void MarkRushed() => _hasRushedOnce = true;

    private bool _needRecoveryAfterRush;
    private bool _needRecoveryAfterMelee;
    public bool NeedRecoveryAfterRush => _needRecoveryAfterRush;
    public bool NeedRecoveryAfterMelee => _needRecoveryAfterMelee;

    private IEnemyAttackPattern _currentPattern;

    private Dictionary<EEnemyHitboxType, EnemyKnockbackHitbox> _hitboxMap;

    private void Awake()
    {
        _enemy = GetComponent<EnemyBase>();
        _movement = GetComponent<EnemyMovement>();
        _anim = GetComponent<EnemyAnimatorController>();
        _agent = GetComponent<NavMeshAgent>();

        CacheHitboxes();
    }

    private void OnEnable()
    {
        // 리스폰 시 공격 상태 초기화
        _currentPattern?.Stop();
        _currentPattern = null;
        _hasRushedOnce = false;
        _needRecoveryAfterRush = false;
        _needRecoveryAfterMelee = false;
    }

    public void Initialize(EnemyCombatContext context)
    {
        _player = context.Player;

        _statData = _enemy != null ? _enemy.EnemyStatData : null;
        _damage = _statData != null ? _statData.Damage : 0f;

        _slotCoordinator = context.SlotCoordinator;
        _attackDirector = context.AttackDirector;

        if (_hitboxMap == null || _hitboxMap.Count == 0)
        {
            // 히트박스 캐싱
            CacheHitboxes();
        }
    }

    private void CacheHitboxes()
    {
        _hitboxMap = new Dictionary<EEnemyHitboxType, EnemyKnockbackHitbox>();

        var hitboxes = GetComponentsInChildren<EnemyKnockbackHitbox>(true);
        foreach (var hb in hitboxes)
        {
            var tag = hb.GetComponent<EnemyHitboxTag>();
            if (tag == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[EnemyAttack] EnemyHitboxTag missing on {hb.name} ({name})");
#endif
                continue;
            }

            if (_hitboxMap.ContainsKey(tag.Type))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[EnemyAttack] Duplicate hitbox type {tag.Type} on {name}");
#endif
                continue;
            }
            _hitboxMap.Add(tag.Type, hb);
        }
    }

    // 컨텍스트가 부르는 히트박스 조회 함수
    public EnemyKnockbackHitbox GetHitbox(EEnemyHitboxType type)
    {
        if (_hitboxMap == null) CacheHitboxes();
        _hitboxMap.TryGetValue(type, out var hitbox);
        return hitbox;
    }


    public void StartAttack()
    {
        if (_enemy != null && _enemy.IsDead) return;
        if (_currentPattern != null) return;

        SelectPattern();
        _currentPattern?.Start();
    }

    public void UpdateAttack()
    {
        _currentPattern?.Update();
    }

    public void Stop()
    {
        _currentPattern?.Stop();
        _currentPattern = null;
    }

    private void SelectPattern()
    {
        var patternContext = new EnemyAttackPatternContext(
            _player,
            transform,
            _damage,
            _movement,
            _anim,
            _agent,
            _slotCoordinator,
            _attackDirector
        );

        switch (_enemy.EnemyStatData.EnemyType)
        {
            case EEnemyType.Normal:
                var normalWaitConfig = new AttackWaitActionConfig(
                    minWait: 999f,
                    maxWait: 999f,
                    waitSpeedMultiplier: 0.9f,
                    releaseSlotOnExit: true,
                    fixedSlotIndex: -1,
                    arrivedThreshold: 0.2f,
                    minStoppingDistance: 0.1f
                );

                var normalConfig = new NormalAttackPatternConfig(
                    pressureWaitConfig: normalWaitConfig,
                    openingRushDuration: 0.3f,
                    meleeCooldownMin: 3f,
                    meleeCooldownMax: 5f,
                    meleeAttackDelay: 0.2f,
                    howlDuration: 3f,
                    knockbackDistance: 2f
                );

                _currentPattern = new NormalAttackPattern(patternContext, normalConfig);
                break;

            case EEnemyType.Small:
                var smallWaitConfig = new AttackWaitActionConfig(
                    minWait: 999f,
                    maxWait: 999f,
                    waitSpeedMultiplier: 0.9f,
                    releaseSlotOnExit: true,
                    fixedSlotIndex: -1,
                    arrivedThreshold: 0.2f,
                    minStoppingDistance: 0.1f
                );

                var smallConfig = new SmallAttackPatternConfig(
                    pressureWaitConfig: smallWaitConfig,
                    meleeCooldownMin: 3f,
                    meleeCooldownMax: 5f,
                    meleeAttackDelay: 0.2f,
                    knockbackDistance: 1f
                );

                _currentPattern = new SmallAttackPattern(patternContext, smallConfig);
                break;

            case EEnemyType.Elite:
                var eliteConfig = new EliteAttackPatternConfig(
                    openingRushDuration: 0.5f,
                    ripRange: 12f,
                    ripMoveSpeed: 2f,
                    ripDamagePerHit: 2f,
                    ripTouchDelay: 0.25f,
                    ripKnockbackDistance: 1f,
                    howlDuration: 3.4f
                );

                _currentPattern = new EliteAttackPattern(patternContext, eliteConfig);
                break;
        }
    }

    #region Animation Event Fowarding

    public void OnMeleeStart() => _currentPattern?.OnAnimEvent(EAttackAnimEvent.MeleeStart);
    public void OnMeleeHitStart() => _currentPattern?.OnAnimEvent(EAttackAnimEvent.MeleeHitStart);
    public void OnMeleeHitEnd() => _currentPattern?.OnAnimEvent(EAttackAnimEvent.MeleeHitEnd);
    public void OnMeleeEnd() => _currentPattern?.OnAnimEvent(EAttackAnimEvent.MeleeEnd);
    public void OnRipStart() => _currentPattern?.OnAnimEvent(EAttackAnimEvent.RipStart);
    public void OnRipHitStart() => _currentPattern?.OnAnimEvent(EAttackAnimEvent.RipHitStart);
    public void OnRipHitEnd() => _currentPattern?.OnAnimEvent(EAttackAnimEvent.RipHitEnd);
    public void OnRipEnd() => _currentPattern?.OnAnimEvent(EAttackAnimEvent.RipEnd);
    public void OnSfxStart() => _currentPattern?.OnAnimEvent(EAttackAnimEvent.SfxStart);

    #endregion

    // 후딜 관련
    public void MarkNeedRecoveryAfterRush() => _needRecoveryAfterRush = true;
    public void ConsumeRecoveryAfterRush() => _needRecoveryAfterRush = false;
    public void MarkNeedRecoveryAfterMelee() => _needRecoveryAfterMelee = true;
    public void ConsumeRecoveryAfterMelee() => _needRecoveryAfterMelee = false;

    // 리셋 관련
    public void ResetRush()
    {
        _hasRushedOnce = false;
    }
}