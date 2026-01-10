using System.Collections.Generic;

public class EliteAttackPattern : IEnemyAttackPattern
{
    private readonly StepAttackPatternRunner _runner;

    public bool IsFinished => false;

    public EliteAttackPattern(EnemyAttackPatternContext context, EliteAttackPatternConfig config)
    {
        // 기본 스텝
        var steps = new List<IEnemyAttackStep>
        {
            new OpeningRushStep(context, config.OpeningRushDistance, config.OpeningRushDuration),
            new EliteRipStep(context, config),
            new HowlingStep(context, config.RipRange, config.HowlDuration)
        };

        // 항상 돌아가는 스텝
        var always = new List<IEnemyAttackAlwaysStep>
        {

        };

        _runner = new StepAttackPatternRunner(steps, always);
    }

    public void Start() => _runner.Start();
    public void Update() => _runner.Update();
    public void Stop() => _runner.Stop();
    public void OnAnimEvent(EAttackAnimEvent animEvent) => _runner.OnAnimEvent(animEvent);
}
