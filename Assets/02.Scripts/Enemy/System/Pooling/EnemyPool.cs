using UnityEngine;
using System.Collections.Generic;

public class EnemyPool : PoolBase<EEnemyType, EnemyBase>
{
    [System.Serializable]
    public class EnemyPoolData
    {
        public EEnemyType Type;
        public EnemyBase Prefab;
        public EnemyStatData StatData;
        public int InitialSize;
    }

    [SerializeField]
    private List<EnemyPoolData> _enemyPools;

    private void Awake()
    {
        foreach (var data in _enemyPools)
        {
            RegisterType(data.Type, data.Prefab, data.InitialSize);
        }
    }

    public EnemyBase SpawnEnemy(EEnemyType type, Vector3 position, Quaternion rotation)
    {
        EnemyBase enemy = Spawn(type, position, rotation);

        enemy.SetPool(this);
        enemy.SetEnemyType(type);

        EnemyStatData statData = GetStatData(type);
        if (statData == null)
        {
            Debug.LogError($"[EnemyPool] StatData가 null입니다! Type: {type} - 스폰 취소");
            Despawn(type, enemy);
            return null;
        }

        enemy.Initialize(statData);
        enemy.GetComponent<EnemyMovement>()?.Initialize();

        enemy.gameObject.SetActive(true);

        return enemy;
    }

    private EnemyStatData GetStatData(EEnemyType type)
    {
        foreach (var data in _enemyPools)
        {
            if (data.Type == type)
                return data.StatData;
        }

        Debug.LogError($"StatData not found for type: {type}");
        return null;
    }

    // 스포너에서 스폰 높이 참고 메서드
    public float GetSpawnHeightForType(EEnemyType type)
    {
        if (_prefabs.TryGetValue(type, out EnemyBase prefab))
        {
            return prefab.GetSpawnHeight();
        }

        Debug.LogError($"적 프리팹을 찾을 수 없습니다: {type}");
        return 0f;
    }
}
