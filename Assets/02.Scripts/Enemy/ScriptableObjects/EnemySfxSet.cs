using UnityEngine;

[CreateAssetMenu(fileName = "EnemySfxSet", menuName = "Enemy/SFX Set")]
public class EnemySfxSet : ScriptableObject
{
    [Header("리액션")]
    public string EnemyHit;
    public string EnemyDeath;

    [Header("공격")]
    public string EnemyHowl;
    public string EnemyMelee;
    public string EnemyRip;
    public string EnemyRush;

    [Header("스폰")]
    public string EnemySpawn;
}
