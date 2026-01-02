using UnityEngine;

public interface IPoolable
{
    // 활성화 시 호출
    void OnSpawn();

    // 비활성화 시 호출
    void OnDespawn();
}
