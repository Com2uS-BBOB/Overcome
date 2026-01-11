using System.Collections;
using UnityEngine;

public class FloatSmallEnemySpawner : MonoBehaviour, IEnemyRespawnRequester
{
    [Header("플레이어")]
    [SerializeField] private Transform _player;

    [Header("풀링")]
    [SerializeField] private EnemyPool _enemyPool;

    [Header("스폰 설정")]
    [SerializeField] private float _respawnDelay = 3f;

    private EnemyCombatContext _enemyCombatContext;

    private void Start()
    {
        var director = _player.GetComponent<EnemyAttackDirector>();
        var slots = _player.GetComponent<EnemySlotCoordinator>();

        _enemyCombatContext = new EnemyCombatContext(_player, director, slots);

        SpawnInitialEnemies();
    }

    private void HandleEnemyDespawn(EnemyBase enemy)
    {
        enemy.OnDespawn -= HandleEnemyDespawn;

        _enemyPool.Despawn(enemy.EnemyType, enemy);
    }

    private void SpawnInitialEnemies()
    {
        // 공중 적 스폰
        Vector3 center = transform.position;
        SpawnEnemy(EEnemyType.FloatSmall, center);
    }

    private void SpawnEnemy(EEnemyType type, Vector3 basePosition)
    {
        Vector3 finalPosition = basePosition + GetSpawnHeight(type);

        EnemyBase enemy = _enemyPool.SpawnEnemy(
            type,
            finalPosition,
            Quaternion.identity
        );

        enemy.SetRespawnRequester(this);
        enemy.SetFloatSmallEnemySpawner(this);
        enemy.SetSpawnBasePosition(basePosition);  // 리스폰용 (높이 재설정 제외)

        enemy.OnDespawn += HandleEnemyDespawn;

        // todo. EnemyBase 활성화 시점으로 위치 이동 예정
        EnemyEventController.Enemy.RaiseSpawned(new EnemySpawnedEvent(enemy));
        EnemyState logic = enemy.GetComponent<EnemyState>();
        logic.Initialize(_enemyCombatContext);
    }

    private Vector3 GetSpawnHeight(EEnemyType type)
    {
        return Vector3.up * _enemyPool.GetSpawnHeightForType(type);
    }

    // 리스폰 요청 처리
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
