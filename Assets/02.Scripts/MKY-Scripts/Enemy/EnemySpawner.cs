using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("풀링")]
    [SerializeField] private EnemyPool _enemyPool;

    [Header("스폰 설정")]
    [SerializeField] private int _aroundCount = 20;
    [SerializeField] private float _spawnRadius = 6f;
    [SerializeField] private float _respawnDelay = 3f;

    [Header("자연스러운 배치 설정")]
    [SerializeField] private float _randomDegree = 10f;
    [SerializeField] private float _randomRadius = 0.5f;

    private void Start()
    {
        SpawnInitialEnemies();
    }

    private void SpawnInitialEnemies()
    {
        // 중앙에 Elite 스폰
        Vector3 center = transform.position;
        SpawnEnemy(EEnemyType.Elite, center);

        // 주위에 적 스폰
        int count = _aroundCount + 1;
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            // 일반 혹은 소형 랜덤 스폰
            EEnemyType type = Random.value < 0.5f
                ? EEnemyType.Normal
                : EEnemyType.Small;

            // 약간의 랜덤 각도 및 반경 변동 추가 (자연스러운 배치)
            float angleDegree = angleStep * i + Random.Range(-_randomDegree, _randomDegree);
            float radius = _spawnRadius + Random.Range(-_randomRadius, _randomRadius);

            float angleRadius = angleDegree * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(
                Mathf.Cos(angleRadius) * radius,
                0f,
                Mathf.Sin(angleRadius) * radius
            );

            Vector3 spawnPosition = center + offset;
            SpawnEnemy(type, spawnPosition);
        }
    }

    private void SpawnEnemy(EEnemyType type, Vector3 basePosition)
    {
        Vector3 finalPosition = basePosition + GetSpawnHeight(type);

        EnemyBase enemy = _enemyPool.SpawnEnemy(
            type,
            finalPosition,
            Quaternion.identity
        );

        enemy.SetSpawner(this);
        enemy.SetSpawnBasePosition(basePosition); // 리스폰용 (높이 재설정 제외)
    }
    private Vector3 GetSpawnHeight(EEnemyType type)
    {
        EnemyBase prefab = _enemyPool.GetPrefab(type);
        return Vector3.up * prefab.GetSpawnHeight();
    }

    public void RequestRespawn(EnemyBase enemy)
    {
        StartCoroutine(RespawnEnemy_Coroutine(enemy));
    }

    private IEnumerator RespawnEnemy_Coroutine(EnemyBase enemy)
    {
        Vector3 respawnPosition = enemy.GetSpawnBasePosition();
        EEnemyType type = enemy.EnemyType;

        yield return new WaitForSeconds(_respawnDelay);

        SpawnEnemy(type, respawnPosition);
    }
}