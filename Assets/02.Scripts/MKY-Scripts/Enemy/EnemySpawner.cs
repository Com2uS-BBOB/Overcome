using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("풀링")]
    [SerializeField] private EnemyPool _enemyPool;

    [Header("스폰 설정")]
    [SerializeField] private int _minAroundCount = 20;
    [SerializeField] private int _maxAroundCount = 20;
    [SerializeField] private float _spawnRadius = 6f;
    [SerializeField] private float _respawnDelay = 3f;

    private void Start()
    {
        SpawnInitialEnemies();
    }

    private void SpawnInitialEnemies()
    {
        // 중앙에 Elite 스폰
        Vector3 center = transform.position;
        SpawnEnemy(EEnemyType.Elite, center);

        // 주변에 적 스폰
        int count = Random.Range(_minAroundCount, _maxAroundCount + 1);

        float angleStep = 360f / count;
        float radius = _spawnRadius;

        for (int i = 0; i < count; i++)
        {
            EEnemyType type = Random.value < 0.5f
                ? EEnemyType.Normal
                : EEnemyType.Small;

            float angle = angleStep * i * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius
            );

            Vector3 spawnPosition = center + offset;
            SpawnEnemy(type, spawnPosition);
        }
    }

    private void SpawnEnemy(EEnemyType type, Vector3 position)
    {
        Vector3 offset = enemyOffset(type);
        Vector3 finalPosition = position + offset;

        EnemyBase enemy = _enemyPool.SpawnEnemy(type, finalPosition, Quaternion.identity);
        enemy.SetSpawner(this);
    }

    private Vector3 enemyOffset(EEnemyType type)
    {
        EnemyBase prefab = _enemyPool.GetPrefab(type);
        return prefab.GetSpawnOffset();
    }

    public void RequestRespawn(EnemyBase enemy)
    {
        StartCoroutine(RespawnEnemy(enemy.EnemyType));
    }

    private IEnumerator RespawnEnemy(EEnemyType type)
    {
        yield return new WaitForSeconds(_respawnDelay);
        SpawnEnemy(type, GetRandomPositionAround());
    }

    private Vector3 GetRandomPositionAround()
    {
        Vector2 circle = Random.insideUnitCircle.normalized * Random.Range(2f, _spawnRadius);
        return transform.position + new Vector3(circle.x, 0f, circle.y);
    }
}