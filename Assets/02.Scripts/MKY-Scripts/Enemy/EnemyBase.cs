using UnityEngine;
using System;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("스탯")]
    public EnemyStatData EnemyStatData;

    protected float _currentHealth;

    [Header("스폰 높이")]
    [SerializeField] private float _spawnHeight = 1f;  // 바닥과 적의 중심 높이 차이

    public EEnemyType EnemyType { get; private set; }

    private EnemyPool _pool;
    private EnemySpawner _spawner;

    private Vector3 _spawnBasePosition;  // 최초 스폰 위치 저장용 (리스폰 때 사용)

    public event Action<float, float> OnHealthChanged;  // 체력UI 갱신용 이벤트
    public event Action<float> OnEnemyHit;              // 적이 맞았을 때 이벤트
    public event Action<EnemyStatData> OnEnemyKilled;  // 적이 죽었을 때 보상 제공용 이벤트

    protected virtual void OnEnable()
    {
        _currentHealth = EnemyStatData.MaxHealth;
        OnHealthChanged?.Invoke(_currentHealth, EnemyStatData.MaxHealth);
    }

    public void SetPool(EnemyPool pool)
    {
        _pool = pool;
    }

    public void SetEnemyType(EEnemyType type)
    {
        EnemyType = type;
    }

    public void SetSpawner(EnemySpawner spawner)
    {
        _spawner = spawner;
    }

    // 최초 스폰 위치 저장 (리스폰용)
    public void SetSpawnBasePosition(Vector3 basePosition)
    {
        _spawnBasePosition = basePosition;
    }

    // 리스폰 위치 반환 + 높이 보정
    public Vector3 GetSpawnBasePosition()
    {
        return _spawnBasePosition;
    }
    public float GetSpawnHeight()
    {
        return _spawnHeight;
    }


    public virtual void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        _currentHealth = Mathf.Max(_currentHealth, 0);

        OnEnemyHit?.Invoke(damage);  // 적이 맞았을 때 이벤트 호출
        OnHealthChanged?.Invoke(_currentHealth, EnemyStatData.MaxHealth);  // 체력UI 갱신

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"적이 죽었습니다.");

        OnEnemyKilled?.Invoke(EnemyStatData);  // EnemyReward에 보상 제공 알림

        _spawner.RequestRespawn(this);
        _pool.Despawn(EnemyType, this);
    }
}