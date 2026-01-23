using UnityEngine;

public class EnemyReward : MonoBehaviour
{
    private EnemyBase _enemy;

    private int _score;
    private float _playtime;

    private void OnEnable()
    {
        EnemyEventController.Enemy.OnKilled += HandleEnemyKilled;
    }

    private void OnDisable()
    {
        EnemyEventController.Enemy.OnKilled -= HandleEnemyKilled;
    }

    private void HandleEnemyKilled(EnemyKilledEvent e)
    {
        _score += e.Score;
        _playtime += e.Playtime;
    }
}
