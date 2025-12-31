using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStat", menuName = "Enemy/Stat")]
public class EnemyStatData : ScriptableObject
{
    public EEnemyType EnemyType;

    [Header("기본 스탯")]
    [SerializeField] private float _maxHealth;
    [SerializeField] private float _health;
    [SerializeField] private float _moveSpeed;

    [Header("보상")]
    [SerializeField] private int _score;
    [SerializeField] private int _playtime;
}