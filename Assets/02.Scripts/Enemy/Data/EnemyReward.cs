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

    private void HandleEnemyKilled(int score, int playtime)
    {
        _score += score;
        _remainingTime += playtime;

        Debug.Log($"보상 획득 → Score +{score}, Time +{playtime}");
    }
}
