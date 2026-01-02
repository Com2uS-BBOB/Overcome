using System.Collections.Generic;
using UnityEngine;

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
}
