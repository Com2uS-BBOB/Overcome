using UnityEngine;

public class EnemyHit : MonoBehaviour
{
    private EnemyBase _enemy;

    private void Awake()
    {
        _enemy = GetComponent<EnemyBase>();
    }

    private void OnEnable()
    {
        _enemy.OnEnemyHit += HandleHit;
    }

    private void OnDisable()
    {
        _enemy.OnEnemyHit -= HandleHit;
    }

    private void HandleHit(float damage)
    {
        Debug.Log($"적 피격! Damage: {damage}");
    }
}
