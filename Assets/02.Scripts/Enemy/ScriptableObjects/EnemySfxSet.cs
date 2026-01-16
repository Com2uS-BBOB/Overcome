using UnityEngine;

[CreateAssetMenu(fileName = "EnemySfxSet", menuName = "Enemy/SFX Set")]
public class EnemySfxSet : ScriptableObject
{
    [Header("리액션")]
    public string EnemyHitSound;
    public string EnemyDeathSound;

    [Header("공격")]
    public string EnemyHowlSound;
    public string EnemyMeleeSound;
    public string EnemyRipSound;
    public string EnemyRushSound;

    [Header("스폰")]
    public string EnemySpawnSound;
}
