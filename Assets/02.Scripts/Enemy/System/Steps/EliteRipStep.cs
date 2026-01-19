using UnityEngine;

public class EliteRipStep : IEnemyAttackStep
{
    private readonly EnemyAttackPatternContext _context;
    private readonly EliteAttackPatternConfig _config;

    private readonly EnemyAttack _attack;

    private EliteRipAction _rip;

    private bool _reserved;
    private float _retryTime;
    private bool _finished;

    public bool IsFinished => _finished;

    public EliteRipStep(EnemyAttackPatternContext context, EliteAttackPatternConfig config)
    {
        _context = context;
        _config = config;
        _attack = _context.Enemy.GetComponent<EnemyAttack>();
    }

    public bool TryStart()
    {
        if (_context.Player == null) return false;

        // 공격권 실패 시 너무 자주 시도하지 않도록 설정
        if (Time.time < _retryTime) return false;

        float distance = Vector3.Distance(_context.Enemy.position, _context.Player.position);
        if (distance > _config.RipRange)  // 범위 밖이면 Rip 시작 금지
        {
            _retryTime = Time.time + _config.RipTouchDelay; // 너무 자주 시도 방지
            return false;
        }

        // 공격권
        bool granted = (_context.AttackDirector == null) || _context.AttackDirector.TryReserve(_context.Enemy);
        if (!granted)
        {
            _retryTime = Time.time + _config.RipTouchDelay;
            return false;
        }
        _reserved = (_context.AttackDirector != null);

        _finished = false;

        string ripKey = _context.SfxSet != null ? _context.SfxSet.EnemyRipSound : null;
        _rip = new EliteRipAction(
            _context.Enemy,
            _context.Player,
            _context.Movement,
            _context.GetHitbox?.Invoke(EEnemyHitboxType.Rip),
            _context.Agent,
            _context.Anim,
            _config.RipDamagePerHit,
            _config.RipMoveSpeed,
            _config.RipKnockbackDistance,
            ripKey
        );
        _rip.Enter();
        return true;
    }

    public void Tick()
    {
        if (_finished) return;

        if (_context.Player == null)
        {
            FinishAndCleanup();
            return;
        }

        float distance = Vector3.Distance(_context.Enemy.position, _context.Player.position);
        if (distance > _config.RipRange)
        {
            _rip?.Exit();
            _rip = null;

            ReleaseAttack();

            // Rush를 다시 가능하게
            _attack?.ResetRush();

            _finished = true;
            return;
        }

        // 범위 안이면 Rip은 계속 유지
        _rip?.Update();
    }

    public void Stop()
    {
        _rip?.Exit();
        _rip = null;

        ReleaseAttack();

        _attack.MarkNeedRecoveryAfterMelee();
        _finished = true;
    }

    private void ReleaseAttack()
    {
        if (_reserved)
        {
            _context.AttackDirector?.Release(_context.Enemy);
            _reserved = false;
        }
    }

    private void FinishAndCleanup()
    {
        _rip?.Exit();
        _rip = null;
        ReleaseAttack();
        _finished = true;
    }

    public void OnAnimEvent(EAttackAnimEvent animEvent)
    {
        if (_rip == null) return;

        switch (animEvent)
        {
            case EAttackAnimEvent.RipStart:
                _rip.OnAnimStart();
                break;
            case EAttackAnimEvent.RipHitStart:
                _rip.OnHitStart();
                break;
            case EAttackAnimEvent.RipHitEnd:
                _rip.OnHitEnd();
                break;
            case EAttackAnimEvent.RipEnd:
                _rip.OnAnimEnd();
                break;
            case EAttackAnimEvent.SfxStart:
                _rip.OnSfxStart();
                break;
        }
    }
}
