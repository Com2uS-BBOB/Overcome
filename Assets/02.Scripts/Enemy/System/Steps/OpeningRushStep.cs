using UnityEngine;
using UnityEngine.AI;

public class OpeningRushStep : IEnemyAttackStep
{
    private readonly EnemyAttackPatternContext _context;
    private readonly float _rushDuration;

    private readonly EnemyAttack _attack; // HasRushedOnce / MarkRushed 사용

    private RushAction _rush;
    private bool _finished;

    public bool IsFinished => _finished;

    public OpeningRushStep(EnemyAttackPatternContext context, float rushDuration)
    {
        _context = context;
        _rushDuration = rushDuration;

        _attack = _context.Enemy.GetComponent<EnemyAttack>();
    }

    public bool TryStart()
    {
        if (_context.Player == null) return false;
        if (_attack == null) return false;

        if (_attack.HasRushedOnce) return false;

        _finished = false;

        string rushKey = _context.SfxSet != null ? _context.SfxSet.EnemyRushSound : null;
        _rush = new RushAction(
            _context.Enemy,
            _context.Player,
            _context.Movement,
            _context.KnockbackHitbox,
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
        if (_rush == null)
        {
            _finished = true;
            return;
        }

        _rush.Update();

        if (_rush.IsFinished)
        {
            _rush.Exit();
            _rush = null;

            _attack.MarkRushed();
            _attack.MarkNeedRecoveryAfterRush();

            _finished = true;
        }
    }

    public void Stop()
    {
        if (_rush != null)
        {
            _rush.Exit();
            _rush = null;
        }
        _finished = true;
    }

    public void OnAnimEvent(EAttackAnimEvent animEvent)
    {
        if (_rush == null) return;

        switch (animEvent)
        {
            case EAttackAnimEvent.SfxStart:
                _rush.OnSfxStart();
                break;
        }
    }
}
