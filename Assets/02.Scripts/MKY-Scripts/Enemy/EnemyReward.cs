using UnityEngine;

public class EnemyReward : MonoBehaviour
{
    private int _score;
    private float _remainingTime;
    
    private void OnEnable()
    {
        EnemyBase.OnEnemyKilled += HandleEnemyKilled;
    }

    private void OnDisable()
    {
        EnemyBase.OnEnemyKilled -= HandleEnemyKilled;
    }

    private void HandleEnemyKilled(EnemyStatData stat)
    {
        _score += stat.Score;
        _remainingTime += stat.Playtime;

        Debug.Log($"보상 획득 → Score +{stat.Score}, Time +{stat.Playtime}");
    }
}
