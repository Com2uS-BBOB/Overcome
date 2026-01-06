using UnityEngine;

public class EnemyReward : MonoBehaviour
{
    private EnemyBase _enemy;

    private int _score;
    private float _remainingTime;

    private void Awake()
    {
        _enemy = GetComponent<EnemyBase>();
    }

    private void OnEnable()
    {
        _enemy.OnEnemyKilled += HandleEnemyKilled;
    }

    private void OnDisable()
    {
        _enemy.OnEnemyKilled -= HandleEnemyKilled;
    }

    private void HandleEnemyKilled(EnemyStatData stat)
    {
        _score += stat.Score;
        _remainingTime += stat.Playtime;

        Debug.Log($"보상 획득 → Score +{stat.Score}, Time +{stat.Playtime}");
    }
}
