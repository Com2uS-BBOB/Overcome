using UnityEngine;

public class EnemySpawnTester : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            PoolManager.Instance
                .GetPool<EnemyPool>()
                .SpawnEnemy(EEnemyType.Normal, transform.position, Quaternion.identity);
        }
    }
}