using UnityEngine;

public class EnemySpawnTester : MonoBehaviour
{
    [Header("풀")]
    [SerializeField] private EnemyPool _enemyPool;

    [Header("스폰 옵션")]
    [SerializeField] private EEnemyType _spawnType = EEnemyType.Normal;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private float _spawnInterval = 3f;

    [Header("자동 스폰")]
    [SerializeField] private bool _autoSpawn = true;

    private float _timer;

    private void Update()
    {
        if (_autoSpawn)
        {
            _timer += Time.deltaTime;
            if (_timer >= _spawnInterval)
            {
                Spawn();
                _timer = 0f;
            }
        }
    }

    private void Spawn()
    {
        if (_enemyPool == null)
        {
            Debug.LogError("[EnemySpawnTester] EnemyPool이 없습니다!");
            return;
        }

        Vector3 position = _spawnPoint != null
            ? _spawnPoint.position
            : transform.position;

        EnemyBase enemy = _enemyPool.SpawnEnemy(
            _spawnType,
            position,
            Quaternion.identity
        );

        Debug.Log($"[SpawnTester] Spawned {enemy.name} ({_spawnType})");
    }
}
