using UnityEngine;

public class EnemyPool : PoolBase<EEnemyType, EnemyBase>
{
    [System.Serializable]
    public struct EnemyPrefab
    {
        public EEnemyType type;
        public EnemyBase prefab;
    }

    [SerializeField] private EnemyPrefab[] _enemyPrefabs;

    protected override void SetupPrefabs()
    {
        foreach (var data in _enemyPrefabs)
        {
            if (!_prefabs.ContainsKey(data.type))
            {
                _prefabs.Add(data.type, data.prefab);
            }
        }
    }

    public EnemyBase SpawnEnemy(EEnemyType type, Vector3 pos, Quaternion rot)
    {
        EnemyBase enemy = Spawn(type, pos, rot);
        enemy.SetPool(this);
        enemy.SetEnemyType(type);
        return enemy;
    }
}
