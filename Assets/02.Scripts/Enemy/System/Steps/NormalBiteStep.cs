using UnityEngine;

public class NormalBiteStep : IEnemyAttackStep
{
    private readonly EnemyAttackPatternContext _context;
    private readonly NormalAttackPatternConfig _config;

    private BiteAction _bite;

    private bool _reserved;
    private float _nextBiteTime;

    public bool IsFinished => _bite == null; // bite 끝나면 null로 만들고 finished 처리

    public NormalBiteStep(EnemyAttackPatternContext context, NormalAttackPatternConfig config)
    {
        _context = context;
        _config = config;
    }

    public bool TryStart()
    {
        if (_context.Player == null) return false;

        // 첫 쿨타임이 설정되지 않았다면(패턴 시작 직후) 한 번만 예약
        if (_nextBiteTime <= 0f)
        {
            _nextBiteTime = Time.time + Random.Range(_config.BiteCooldownMin, _config.BiteCooldownMax);
        }

        // 아직 쿨타임이면 시작 못함
        if (Time.time < _nextBiteTime) return false;

        // 공격권
        bool granted = (_context.AttackDirector == null) || _context.AttackDirector.TryReserve(_context.Enemy);
        if (!granted)
        {
            _nextBiteTime = Time.time + _config.BiteTouchDelay;
            return false;
        }

        _reserved = (_context.AttackDirector != null);

        _bite = new BiteAction(
            _context.Enemy,
            _context.Player,
            _context.KnockbackHitbox,
            _context.Animator,
            _context.Agent,
            _context.Damage
        );
        _bite.Enter();
        return true;
    }

    public void Tick()
    {
        if (_bite == null) return;

        _bite.Update();

        if (_bite.IsFinished)
        {
            _bite.Exit();
            _bite = null;

            if (_reserved)
            {
                _context.AttackDirector?.Release(_context.Enemy);
                _reserved = false;
            }

            _nextBiteTime = Time.time + Random.Range(_config.BiteCooldownMin, _config.BiteCooldownMax);
        }
    }

    public void Stop()
    {
        if (_bite != null)
        {
            _bite.Exit();
            _bite = null;
        }

        if (_reserved)
        {
            _context.AttackDirector?.Release(_context.Enemy);
            _reserved = false;
        }
    }

    public void OnAnimEvent(EAttackAnimEvent animEvent)
    {
        if (_bite == null) return;

        switch (animEvent)
        {
            case EAttackAnimEvent.BiteStart:
                _bite.OnAnimStart();
                break;
            case EAttackAnimEvent.BiteHitStart:
                _bite.OnHitStart();
                break;
            case EAttackAnimEvent.BiteHitEnd:
                _bite.OnHitEnd();
                break;
            case EAttackAnimEvent.BiteEnd:
                _bite.OnAnimEnd();
                break;
        }
    }
}
