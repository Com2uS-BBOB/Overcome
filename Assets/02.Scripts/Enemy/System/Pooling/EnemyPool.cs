using UnityEngine;
using System.Collections.Generic;

public class EnemyPool : PoolBase<EEnemyType, EnemyBase>
{
    [System.Serializable]
    public class EnemyPoolData
    {
        public EEnemyType Type;
        public EnemyBase Prefab;
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
        return enemy;
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
