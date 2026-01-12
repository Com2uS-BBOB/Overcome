using UnityEngine;
using UnityEngine.AI;

public class RecoveryStep : IEnemyAttackStep
{
    private readonly EnemyAttackPatternContext _context;
    private readonly EnemyAttack _attack;

    private float _recoveryDuration = 0.3f;
    private float _minDuration = 0.001f;
    private float _time;
    private bool _running;

    public bool IsFinished => _running && (_time >= _recoveryDuration);

    public RecoveryStep(EnemyAttackPatternContext context)
    {
        _context = context;
        _attack = _context.Enemy.GetComponent<EnemyAttack>();
    }

    public bool TryStart()
    {
        // duration이 0이면 시작할 필요 없음
        if (_recoveryDuration <= _minDuration) return false;

        if (_attack == null) return false;

        // 후딜이 필요하지 않으면 시작할 필요 없음
        if (!_attack.NeedRecoveryAfterRush && !_attack.NeedRecoveryAfterMelee)
        {
            return false;
        }

        // 일회성 처리
        if (_attack.NeedRecoveryAfterRush)
        {
            _attack.ConsumeRecoveryAfterRush();
        }
        if (_attack.NeedRecoveryAfterMelee)
        {
            _attack.ConsumeRecoveryAfterMelee();
        }

        _running = true;
        _time = 0f;

        // 후딜 중엔 멈춰있기
        if (_context.Agent != null && _context.Agent.enabled)
        {
            _context.Agent.isStopped = true;
            _context.Agent.ResetPath();
        }

        return true;
    }

    public void Tick()
    {
        if (!_running) return;
        _time += Time.deltaTime;
    }

    public void Stop()
    {
        _running = false;

        // 후딜 끝나면 다시 움직일 수 있게 풀어줌
        if (_context.Agent != null && _context.Agent.enabled)
        {
            _context.Agent.isStopped = false;
        }
    }

    public void OnAnimEvent(EAttackAnimEvent animEvent) { }
}
