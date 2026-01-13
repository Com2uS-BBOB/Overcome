using UnityEngine;

public class EliteRipStep : IEnemyAttackStep
{
    private readonly EnemyAttackPatternContext _context;
    private readonly EliteAttackPatternConfig _config;

    private readonly EnemyAttack _attack;

    private EliteRipAction _rip;
    private HowlAction _howl;

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

        // 공격권
        bool granted = (_context.AttackDirector == null) || _context.AttackDirector.TryReserve(_context.Enemy);
        if (!granted)
        {
            _retryTime = Time.time + _config.RipTouchDelay;
            return false;
        }
        _reserved = (_context.AttackDirector != null);

        _finished = false;

        _rip = new EliteRipAction(
            _context.Enemy,
            _context.Player,
            _context.Movement,
            _context.KnockbackHitbox,
            _context.Agent,
            _context.Animator,
            _config.RipDamagePerHit,
            _config.RipMoveSpeed,
            _config.RipKnockbackDistance
        );
        _rip.Enter();
        return true;
    }

    public void Tick()
    {
        if (_finished) return;

        if (_context.Player == null)
        {
            FinishNow();
            return;
        }

        // 난도질 도중 플레이어가 범위 밖으로 벗어나면 Howl 후 Rush 재진입
        if (_rip != null && Vector3.Distance(_context.Enemy.position, _context.Player.position) > _config.RipRange)
        {
            // Rip 중단
            _rip.Exit();
            _rip = null;

            // 공격권 반납(연출 중에는 공격자로 잡고 있을 필요 없음)
            ReleaseAttack();

            // Howl 시작
            _howl = new HowlAction(_context.Animator, _context.Agent, _config.HowlDuration);
            _howl.Enter();

            return;
        }

        // 포효 진행 중
        if (_howl != null)
        {
            _howl.Update();
            if (_howl.IsFinished)
            {
                _howl.Exit();
                _howl = null;

                // Rush를 다시 가능하게 만든다
                _attack?.ResetRush();

                FinishNow();
            }
            return;
        }

        // 난도질 진행
        if (_rip != null)
        {
            _rip.Update();
            if (_rip.IsFinished)
            {
                _rip.Exit();
                _rip = null;

                ReleaseAttack();
                FinishNow();
            }
        }
        else
        {
            FinishNow();
        }
    }

    public void Stop()
    {
        _rip?.Exit();
        _rip = null;

        _howl?.Exit();
        _howl = null;

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

    private void FinishNow()
    {
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
        }
    }
}
