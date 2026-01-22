using UnityEngine;
using System.Collections;

public class SingleEnemySpawner : MonoBehaviour, IEnemyDespawnHandler
{
    [Header("플레이어")]
    [SerializeField] private Transform _player;
    [SerializeField] private EnemyAttackDirector _attackDirector;
    [SerializeField] private EnemySlotCoordinator _slots;

    [Header("풀링")]
    [SerializeField] private EnemyPool _enemyPool;

    [Header("스폰할 적 타입")]
    [SerializeField] private EEnemyType _spawnEnemyType;

    [Header("스폰 설정")]
    [SerializeField] private float _respawnDelay = 10f;

    private EnemyCombatContext _enemyCombatContext;
    private Coroutine _respawnCoroutine;

    private bool _gameOverStopped;

    private void Start()
    {
        _enemyCombatContext = new EnemyCombatContext(_player, _attackDirector, _slots);

        SpawnAt(transform.position);
    }

    protected virtual void OnEnable()
    {
        GameEventHandler.OnGameEnd += HandleGameOver;
        _gameOverStopped = false;
    }

    protected virtual void OnDisable()
    {
        // GameOver 해제
        GameEventHandler.OnGameEnd -= HandleGameOver;
    }

    private void HandleGameOver()
    {
        if (_gameOverStopped) return;
        _gameOverStopped = true;

#if UNITY_EDITOR
        Debug.Log("게임 오버로 적 스폰 정지");
#endif
    }

    private void SpawnAt(Vector3 basePosition)
    {
        if (_gameOverStopped) return;
        Vector3 finalPosition = basePosition + Vector3.up * _enemyPool.GetSpawnHeightForType(_spawnEnemyType);

        EnemyBase enemy = _enemyPool.SpawnEnemy(_spawnEnemyType, finalPosition, Quaternion.identity);
        if (enemy == null) return;

        enemy.SetDespawnHandler(this);             // 디스폰 처리자로 등록
        enemy.SetSpawnBasePosition(basePosition);  // 리스폰 자리 저장

        EnemyEventController.Enemy.RaiseSpawned(new EnemySpawnedEvent(enemy));
        var logic = enemy.GetComponent<EnemyState>();
        logic?.Initialize(_enemyCombatContext);
    }

    public void HandleDespawn(EnemyBase enemy)
    {
        // 풀에 반환
        _enemyPool.Despawn(enemy.EnemyType, enemy);

        // 리스폰
        if (_respawnCoroutine != null) 
        {
            StopCoroutine(_respawnCoroutine);
        }

        if (_gameOverStopped) return;
        _respawnCoroutine = StartCoroutine(Respawn_Coroutine(enemy.GetSpawnBasePosition()));
    }
    private IEnumerator Respawn_Coroutine(Vector3 basePosition)
    {
        yield return new WaitForSeconds(_respawnDelay);
        SpawnAt(basePosition);
    }
}
