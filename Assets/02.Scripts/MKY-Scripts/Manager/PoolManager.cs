using UnityEngine;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    [System.Serializable]
    public class EnemyPoolData
    {
        public EEnemyType Type;
        public EnemyBase Prefab;
        public int InitialCount;
    }

    public List<EnemyPoolData> EnemyPoolDatas;

    private Dictionary<EEnemyType, Queue<EnemyBase>> _pools = new();
    private Dictionary<EEnemyType, EnemyBase> _prefabs = new();

    private void Awake()
    {
        Instance = this;

        foreach (var data in EnemyPoolDatas)
        {
            Queue<EnemyBase> pool = new Queue<EnemyBase>();

            for (int i = 0; i < data.InitialCount; i++)
            {
                EnemyBase enemy = CreateEnemy(data.Type, data.Prefab);
                enemy.gameObject.SetActive(false);
                pool.Enqueue(enemy);
            }

            _pools.Add(data.Type, pool);
            _prefabs.Add(data.Type, data.Prefab);
        }
    }

    private EnemyBase CreateEnemy(EEnemyType type, EnemyBase prefab)
    {
        EnemyBase enemy = Instantiate(prefab, transform);
        enemy.Init(this, type);
        return enemy;
    }

    public EnemyBase SpawnEnemy(EEnemyType type, Vector3 position)
    {
        EnemyBase enemy;

        if (_pools[type].Count > 0)
        {
            enemy = _pools[type].Dequeue();
        }
        else
        {
            enemy = CreateEnemy(type, _prefabs[type]);
        }

        enemy.transform.position = position;
        enemy.gameObject.SetActive(true);

        return enemy;
    }

    public void ReturnEnemy(EnemyBase enemy)
    {
        enemy.gameObject.SetActive(false);
        _pools[enemy.EnemyType].Enqueue(enemy);
    }
}
