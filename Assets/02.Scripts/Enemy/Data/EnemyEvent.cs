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

    public EnemyHitEvent(EnemyBase enemy, float damage)
    {
        Enemy = enemy;
        Damage = damage;
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