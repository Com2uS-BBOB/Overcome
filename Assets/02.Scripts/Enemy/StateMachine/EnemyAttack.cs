using UnityEngine;
using UnityEngine.AI;

public class EnemyAttack : MonoBehaviour
{
    private Transform _player;
    private EnemyBase _enemy;
    private float _damage;
    private EnemyMovement _movement;
    private EnemyKnockbackHitbox _knockbackHitbox;
    private Animator _animator;
    private EnemySlotCoordinator _slotCoordinator;
    private EnemyAttackDirector _attackDirector;
    private NavMeshAgent _agent;

    private bool _hasRushedOnce;
    public bool HasRushedOnce => _hasRushedOnce;
    public void MarkRushed() => _hasRushedOnce = true;

    private bool _needRecoveryAfterRush;
    private bool _needRecoveryAfterMelee;
    public bool NeedRecoveryAfterRush => _needRecoveryAfterRush;
    public bool NeedRecoveryAfterMelee => _needRecoveryAfterMelee;

    private IEnemyAttackPattern _currentPattern;

    private void Awake()
    {
        _enemy = GetComponent<EnemyBase>();
        _movement = GetComponent<EnemyMovement>();
        _knockbackHitbox = GetComponentInChildren<EnemyKnockbackHitbox>();
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
    }

    public void Initialize(EnemyCombatContext context)
    {
        _player = context.Player;
        _damage = _enemy.EnemyStatData.Damage;

        _slotCoordinator = context.SlotCoordinator;
        _attackDirector = context.AttackDirector;
    }

    public void StartAttack()
    {
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
            _knockbackHitbox,
            _animator,
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
                    waitSpeedMultiplier: 0.15f,
                    releaseSlotOnExit: true,
                    fixedSlotIndex: -1,
                    arrivedThreshold: 0.2f,
                    minStoppingDistance: 0.1f
                );

                var normalConfig = new NormalAttackPatternConfig(
                    pressureWaitConfig: normalWaitConfig,
                    openingRushDuration: 0.4f,
                    meleeCooldownMin: 3f,
                    meleeCooldownMax: 5f,
                    meleeAttackDelay: 0.2f
                );

                _currentPattern = new NormalAttackPattern(patternContext, normalConfig);
                break;

            case EEnemyType.Small:
                var smallWaitConfig = new AttackWaitActionConfig(
                    minWait: 999f,
                    maxWait: 999f,
                    waitSpeedMultiplier: 0.15f,
                    releaseSlotOnExit: true,
                    fixedSlotIndex: -1,
                    arrivedThreshold: 0.2f,
                    minStoppingDistance: 0.1f
                );

                var smallConfig = new SmallAttackPatternConfig(
                    pressureWaitConfig: smallWaitConfig,
                    meleeCooldownMin: 3f,
                    meleeCooldownMax: 5f,
                    meleeAttackDelay: 0.2f
                );

                _currentPattern = new SmallAttackPattern(patternContext, smallConfig);
                break;

            case EEnemyType.Elite:
                var eliteConfig = new EliteAttackPatternConfig(
                    openingRushDuration: 0.2f,
                    ripRange: 5f,
                    ripMoveSpeed: 2f,
                    ripDamagePerHit: 2f,
                    ripTouchDelay: 0.25f,
                    howlDuration: 2.8f
                );

                _currentPattern = new EliteAttackPattern(patternContext, eliteConfig);
                break;
        }
    }

    // 애니메이션 이벤트 포워딩
    public void OnMeleeStart() => _currentPattern?.OnAnimEvent(EAttackAnimEvent.MeleeStart);
    public void OnMeleeHitStart() => _currentPattern?.OnAnimEvent(EAttackAnimEvent.MeleeHitStart);
    public void OnMeleeHitEnd() => _currentPattern?.OnAnimEvent(EAttackAnimEvent.MeleeHitEnd);
    public void OnMeleeEnd() => _currentPattern?.OnAnimEvent(EAttackAnimEvent.MeleeEnd);

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