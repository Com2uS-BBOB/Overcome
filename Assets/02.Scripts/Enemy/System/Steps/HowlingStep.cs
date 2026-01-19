using UnityEngine;

public class HowlingStep : IEnemyAttackStep
{
    private readonly EnemyAttackPatternContext _context;
    private readonly float _howlDuration;
    private readonly EnemyAttack _attack;
    private HowlAction _howl;

    private bool _finished;

    private readonly float _triggerRange;
    private float _nextAllowedTime;

    public bool IsFinished => _finished;

    public HowlingStep(
        EnemyAttackPatternContext context,
        float triggerRange,
        float duration
    )
    {
        _context = context;

        _triggerRange = triggerRange;
        _howlDuration = duration;

        _attack = _context.Enemy.GetComponent<EnemyAttack>();
    }

    public bool TryStart()
    {
        if (_howl != null) return false;
        if (_context.Player == null) return false;
        if (Time.time < _nextAllowedTime) return false;

        // 플레이어가 일정 거리 밖이면 포효
        float distance = Vector3.Distance(_context.Enemy.position, _context.Player.position);
        if (distance <= _triggerRange) return false;

        _finished = false;

        _howl = new HowlAction(_context.Anim, _context.Agent, _context.Movement, _howlDuration);
        _howl.Enter();

        return true;
    }

    public void Tick()
    {
        if (_howl == null)
        {
            _finished = true;
            return;
        }

        _howl.Update();

        if (_howl.IsFinished)
        {
            _howl.Exit();
            _howl = null;

            // 포효 끝나면 다음에 OpeningRush가 다시 발동
            _attack?.ResetRush();

            _nextAllowedTime = Time.time + _howlDuration;

            _finished = true;
        }
    }

    public void Stop()
    {
        if (_howl != null)
        {
            _howl.Exit();
            _howl = null;
        }
        _finished = true;
    }

    public void OnAnimEvent(EAttackAnimEvent animEvent)
    {
        // 포효 애니메이션 이벤트 처리 필요시 여기에 작성
    }
}