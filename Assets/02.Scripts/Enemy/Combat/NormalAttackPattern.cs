using UnityEngine;
using System.Collections.Generic;

public class NormalAttackPattern : IEnemyAttackPattern
{
    private readonly StepAttackPatternRunner _runner;

    public bool IsFinished => false;

    public NormalAttackPattern(EnemyAttackPatternContext context, NormalAttackPatternConfig config)
    {
        // 기본 스텝
        var steps = new List<IEnemyAttackStep>
        {
            new HowlingStep(context, config.MeleeAttackRange, config.HowlDuration),
            new OpeningRushStep(context, config.OpeningRushDistance, config.OpeningRushDuration),
            new MeleeStep(context, config.MeleeCooldownMin, config.MeleeCooldownMax, config.MeleeAttackDelay),
        };

        // 항상 돌아가는 스텝
        var always = new List<IEnemyAttackAlwaysStep>
        {
            new PressureWaitAlwaysStep(context, config.PressureWaitConfig),
        };

        _runner = new StepAttackPatternRunner(steps, always);
    }

    public void Start() => _runner.Start();
    public void Update() => _runner.Update();
    public void Stop() => _runner.Stop();
    public void OnAnimEvent(EAttackAnimEvent animEvent) => _runner.OnAnimEvent(animEvent);
}
