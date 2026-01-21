using UnityEngine;

public class RushStep : IEnemyAttackStep
{
    private readonly EnemyAttackPatternContext _context;

    private readonly float _cooldownMin;
    private readonly float _cooldownMax;
    private readonly float _attackDelay;
    private readonly float _rushDuration;

    private readonly EnemyAttack _attack;

    private RushAction _rush;

    private bool _reserved;
    private float _nextTime;

    public bool IsFinished => _rush == null;

    public RushStep(
        EnemyAttackPatternContext context,
        float cooldownMin,
        float cooldownMax,
        float attackDelay,
        float rushDuration
    )
    {
        _context = context;
        _cooldownMin = cooldownMin;
        _cooldownMax = cooldownMax;
        _attackDelay = attackDelay;
        _rushDuration = rushDuration;

        _attack = _context.Enemy.GetComponent<EnemyAttack>();
    }

    public bool TryStart()
    {
        if (_context.Player == null)
        {
            return false;
        }

        if (_nextTime <= 0f)
        {
            _nextTime = Time.time + Random.Range(_cooldownMin, _cooldownMax);
        }

        if (Time.time < _nextTime)
        {
            return false;
        }


        // 공격권
        bool granted = (_context.AttackDirector == null) || _context.AttackDirector.TryReserve(_context.Enemy);
        if (!granted)
        {
            _nextTime = Time.time + _attackDelay;
            return false;
        }
        _reserved = (_context.AttackDirector != null);

        string rushKey = _context.SfxSet != null ? _context.SfxSet.EnemyRushSound : null;
        _rush = new RushAction(
            _context.Enemy,
            _context.Player,
            _context.Movement,
            _context.GetHitbox?.Invoke(EEnemyHitboxType.Rush),
            _context.Agent,
            _context.Anim,
            _rushDuration,
            _context.Damage,
            rushKey
        );

        _rush.Enter();
        return true;
    }

    public void Tick()
    {
        if (_rush == null) return;

        _rush.Update();

        if (_rush.IsFinished)
        {
            CleanupRush();

            if (_reserved)
            {
                _context.AttackDirector?.Release(_context.Enemy);
                _reserved = false;
            }

            _attack?.MarkNeedRecoveryAfterRush();

            _nextTime = Time.time + Random.Range(_cooldownMin, _cooldownMax);
        }
    }

    public void Stop()
    {
        if (_rush != null)
        {
            CleanupRush();
        }

        if (_reserved)
        {
            _context.AttackDirector?.Release(_context.Enemy);
            _reserved = false;
        }
    }

    private void CleanupRush()
    {
        _rush.Exit();
        _rush = null;
    }

    public void OnAnimEvent(EAttackAnimEvent animEvent) { }
}

