using UnityEngine;

public class MeleeStep : IEnemyAttackStep
{
    private readonly EnemyAttackPatternContext _context;

    private readonly float _cooldownMin;
    private readonly float _cooldownMax;
    private readonly float _attackDelay;
    private readonly float _knockbackDistance;
    private readonly EnemyAttack _attack;

    private MeleeAction _melee;

    private bool _reserved;
    private float _nextAttackTime;

    public bool IsFinished => _melee == null; // 공격 끝나면 null로 만들고 finished 처리

    public MeleeStep(EnemyAttackPatternContext context, float cooldownMin, float cooldownMax, float attackDelay, float knockbackDistance)
    {
        _context = context;
        _cooldownMin = cooldownMin;
        _cooldownMax = cooldownMax;
        _attackDelay = attackDelay;
        _knockbackDistance = knockbackDistance;

        _attack = _context.Enemy.GetComponent<EnemyAttack>();
    }

    public bool TryStart()
    {
        if (_context.Player == null) return false;

        // 첫 쿨타임이 설정되지 않았다면(패턴 시작 직후) 한 번만 예약
        if (_nextAttackTime <= 0f)
        {
            _nextAttackTime = Time.time + Random.Range(_cooldownMin, _cooldownMax);
        }

        // 아직 쿨타임이면 시작 못함
        if (Time.time < _nextAttackTime) return false;

        // 공격권
        bool granted = (_context.AttackDirector == null) || _context.AttackDirector.TryReserve(_context.Enemy);
        if (!granted)
        {
            _nextAttackTime = Time.time + _attackDelay;
            return false;
        }

        _reserved = (_context.AttackDirector != null);

        _melee = new MeleeAction(
            _context.Enemy,
            _context.Player,
            _context.KnockbackHitbox,
            _context.Animator,
            _context.Agent,
            _context.Damage,
            _knockbackDistance
        );
        _melee.Enter();
        return true;
    }

    public void Tick()
    {
        if (_melee == null) return;

        _melee.Update();

        if (_melee.IsFinished)
        {
            _melee.Exit();
            _melee = null;

            if (_reserved)
            {
                _context.AttackDirector?.Release(_context.Enemy);
                _reserved = false;
            }

            _nextAttackTime = Time.time + Random.Range(_cooldownMin, _cooldownMax);
        }
    }

    public void Stop()
    {
        if (_melee != null)
        {
            _melee.Exit();
            _melee = null;
        }

        if (_reserved)
        {
            _context.AttackDirector?.Release(_context.Enemy);
            _reserved = false;
        }

        _attack.MarkNeedRecoveryAfterMelee();
    }

    public void OnAnimEvent(EAttackAnimEvent animEvent)
    {
        if (_melee == null) return;

        switch (animEvent)
        {
            case EAttackAnimEvent.MeleeStart:
                _melee.OnAnimStart();
                break;
            case EAttackAnimEvent.MeleeHitStart:
                _melee.OnHitStart();
                break;
            case EAttackAnimEvent.MeleeHitEnd:
                _melee.OnHitEnd();
                break;
            case EAttackAnimEvent.MeleeEnd:
                _melee.OnAnimEnd();
                break;
        }
    }
}
