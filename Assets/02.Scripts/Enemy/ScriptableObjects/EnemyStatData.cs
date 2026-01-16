using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStat", menuName = "Enemy/Stat")]
public class EnemyStatData : ScriptableObject
{
    public EEnemyType EnemyType;
    public EEnemyDeathMode EnemyDeathMode;

    [Header("기본 스탯")]
    [SerializeField] private float _maxHealth;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _damage;

    public float MaxHealth => _maxHealth;
    public float MoveSpeed => _moveSpeed;
    public float Damage => _damage;

    [Header("보상")]
    [SerializeField] private int _score;
    [SerializeField] private int _playtime;

    public int Score => _score;
    public int Playtime => _playtime;

    [Header("SFX Set")]
    [SerializeField] private EnemySfxSet _enemySfxSet;
    public EnemySfxSet EnemySfxSet => _enemySfxSet;
}