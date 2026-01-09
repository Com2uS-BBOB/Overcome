using UnityEngine;

public struct EliteAttackPatternConfig
{
    public float OpeningRushDistance;
    public float OpeningRushDuration;

    public float RipRange;
    public float RipMoveSpeed;
    public int RipHits;
    public float RipHitInterval;
    public float RipDamagePerHit;       // -2s 느낌이면 데미지 환산
    public float RipTouchDelay;         // 공격권 실패 시 딜레이

    public float HowlDuration;

    public EliteAttackPatternConfig(
        float openingRushDistance = 20f,
        float openingRushDuration = 0.2f,
        float ripRange = 5f,
        float ripMoveSpeed = 2f,
        int ripHits = 5,
        float ripHitInterval = 0.25f,
        float ripDamagePerHit = 1f,
        float ripTouchDelay = 0.25f,
        float howlDuration = 0.6f
    )
    {
        OpeningRushDistance = openingRushDistance;
        OpeningRushDuration = openingRushDuration;
        RipRange = ripRange;
        RipMoveSpeed = ripMoveSpeed;
        RipHits = ripHits;
        RipHitInterval = ripHitInterval;
        RipDamagePerHit = ripDamagePerHit;
        RipTouchDelay = ripTouchDelay;
        HowlDuration = howlDuration;
    }
}
