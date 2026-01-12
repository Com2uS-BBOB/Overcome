using UnityEngine;

public struct EliteAttackPatternConfig
{
    public float OpeningRushDuration;

    public float RipRange;
    public float RipMoveSpeed;
    public float RipDamagePerHit;
    public float RipTouchDelay;         // 공격권 실패 시 딜레이

    public float HowlDuration;

    public float RipKnockbackDistance;

    public EliteAttackPatternConfig(
        float openingRushDuration = 3.2f,
        float ripRange = 10f,
        float ripMoveSpeed = 2f,
        float ripDamagePerHit = 2f,
        float ripTouchDelay = 0.25f,
        float howlDuration = 3.2f,
        float ripKnockbackDistance = 1f
    )
    {
        OpeningRushDuration = openingRushDuration;
        RipRange = ripRange;
        RipMoveSpeed = ripMoveSpeed;
        RipDamagePerHit = ripDamagePerHit;
        RipTouchDelay = ripTouchDelay;
        HowlDuration = howlDuration;
        RipKnockbackDistance = ripKnockbackDistance;
    }
}
