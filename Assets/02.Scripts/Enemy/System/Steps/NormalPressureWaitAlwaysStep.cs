
public class NormalPressureWaitAlwaysStep : IEnemyAttackAlwaysStep
{
    private readonly EnemyAttackPatternContext _context;
    private readonly AttackWaitActionConfig _config;

    private AttackWaitAction _pressure;

    public NormalPressureWaitAlwaysStep(EnemyAttackPatternContext context, AttackWaitActionConfig config)
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
            _config
        );
        _pressure.Enter();
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
}
