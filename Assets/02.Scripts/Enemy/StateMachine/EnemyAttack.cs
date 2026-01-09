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
                var waitConfig = new AttackWaitActionConfig(
                    minWait: 999f,
                    maxWait: 999f,
                    waitSpeedMultiplier: 0.15f,
                    releaseSlotOnExit: true,
                    fixedSlotIndex: -1,
                    arrivedThreshold: 0.2f,
                    minStoppingDistance: 0.1f
                );

                var normalConfig = new NormalAttackPatternConfig(
                    pressureWaitConfig: waitConfig,
                    biteCooldownMin: 3f,
                    biteCooldownMax: 5f,
                    biteTouchDelay: 0.2f
                );

                _currentPattern = new NormalAttackPattern(patternContext, normalConfig);
                break;

                // TODO: Elite, Small 확장
        }
    }

    // 애니메이션 이벤트 포워딩
    public void OnBiteStart() => _currentPattern?.OnAnimEvent(EAttackAnimEvent.BiteStart);
    public void OnBiteHitStart() => _currentPattern?.OnAnimEvent(EAttackAnimEvent.BiteHitStart);
    public void OnBiteHitEnd() => _currentPattern?.OnAnimEvent(EAttackAnimEvent.BiteHitEnd);
    public void OnBiteEnd() => _currentPattern?.OnAnimEvent(EAttackAnimEvent.BiteEnd);

    public void ResetRush()
    {
        _hasRushedOnce = false;
    }
}