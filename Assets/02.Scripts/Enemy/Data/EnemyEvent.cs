using UnityEngine;

public readonly struct EnemyKilledEvent
{
    public readonly EnemyBase Enemy;
    public readonly int Score;
    public readonly int Playtime;

    public EnemyKilledEvent(EnemyBase enemy)
    {
        Enemy = enemy;
        Score = enemy.Score;
        Playtime = enemy.Playtime;
    }
}

public readonly struct EnemyHitEvent
{
    public readonly EnemyBase Enemy;
    public readonly float Damage;
    public readonly GameObject Attacker;

    public EnemyHitEvent(EnemyBase enemy, float damage, GameObject attacker)
    {
        Enemy = enemy;
        Damage = damage;
        Attacker = attacker;
    }
}

public readonly struct EnemySpawnedEvent
{
    public readonly EnemyBase Enemy;
    public readonly EEnemyType Type;

    public EnemySpawnedEvent(EnemyBase enemy)
    {
        Enemy = enemy;
        Type = enemy.EnemyType;
    }
}