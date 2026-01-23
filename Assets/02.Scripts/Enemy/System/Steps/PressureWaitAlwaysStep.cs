using UnityEngine;

public class PressureWaitAlwaysStep : IEnemyAttackAlwaysStep
{
    private readonly EnemyAttackPatternContext _context;
    private readonly AttackWaitActionConfig _config;

    private AttackWaitAction _pressure;
    private bool _wasPaused;

    public PressureWaitAlwaysStep(EnemyAttackPatternContext context, AttackWaitActionConfig config)
    {
        _context = context;
        _config = config;
    }

    public void StartAlways()
    {
        if (_context.Player == null) return;

        _pressure = new AttackWaitAction(
            _context.Enemy,
            _context.Player,
            _context.Movement,
            _context.SlotCoordinator,
            _context.Agent,
            _context.Anim,
            _config
        );
        _pressure.Enter();
        _wasPaused = false;
    }

    public void TickAlways()
    {
        if (_context.Player == null) return;
        _pressure?.Update();
    }

    public void StopAlways()
    {
        _pressure?.Exit();
        _pressure = null;
    }

    public bool ShouldTickWhile(IEnemyAttackStep currentStep)
    {
        // 다음 스텝들일 때 일시정지
        bool shouldPause = 
            (currentStep is HowlingStep) || 
            (currentStep is OpeningRushStep) ||
            (currentStep is RecoveryStep) || 
            (currentStep is MeleeStep) ||
            (currentStep is EliteRipStep);

        // 상태가 바뀌면 Exit/Enter 처리
        if (shouldPause && !_wasPaused)
        {
            // 일시정지 시 Exit하여 NavMeshAgent 제어 해제
            _pressure?.Exit();
            _wasPaused = true;
            return false;
        }
        else if (!shouldPause && _wasPaused)
        {
            // 다시 Enter
            if (_pressure != null && _context.Player != null)
            {
                _pressure.Enter();
            }
            _wasPaused = false;
            return true;
        }
        
        return !shouldPause;
    }
}
