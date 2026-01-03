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

    public EnemyBase GetPrefab(EEnemyType type)
    {
        if (_prefabs.TryGetValue(type, out EnemyBase prefab))
            return prefab;

        Debug.LogError($"Enemy prefab not found: {type}");
        return null;
    }
}
