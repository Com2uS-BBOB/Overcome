using UnityEngine;

public struct NormalAttackPatternConfig
{
    public AttackWaitActionConfig PressureWaitConfig;

    public float OpeningRushDistance;
    public float OpeningRushDuration;

    public float MeleeCooldownMin;
    public float MeleeCooldownMax;
    public float MeleeAttackDelay;
    public float MeleeAttackRange;

    public float HowlDuration;

    public NormalAttackPatternConfig(
        AttackWaitActionConfig pressureWaitConfig,
        float openingRushDistance = 10f,
        float openingRushDuration = 0.4f,
        float meleeCooldownMin = 3f,
        float meleeCooldownMax = 5f,
        float meleeAttackDelay = 0.2f,
        float meleeAttackRange = 5f,
        float howlDuration = 2.4f
    )
    {
        PressureWaitConfig = pressureWaitConfig;
        OpeningRushDistance = openingRushDistance;
        OpeningRushDuration = openingRushDuration;
        MeleeCooldownMin = meleeCooldownMin;
        MeleeCooldownMax = meleeCooldownMax;
        MeleeAttackDelay = meleeAttackDelay;
        MeleeAttackRange = meleeAttackRange;
        HowlDuration = howlDuration;
    }
}
