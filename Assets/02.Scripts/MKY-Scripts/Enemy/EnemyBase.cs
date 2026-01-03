using UnityEngine;
using System;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("스탯")]
    public EnemyStatData EnemyStatData;

    protected float _currentHealth;
    // protected EnemyMovement _movement;

    [Header("스폰 높이")]
    [SerializeField] private float _spawnHeight = 1f;

    public EEnemyType EnemyType { get; private set; }

    private EnemyPool _pool;
    private EnemySpawner _spawner;

    private Vector3 _spawnBasePosition;

    public event Action<float, float> OnHealthChanged;

    protected virtual void OnEnable()
    {
        // _movement = GetComponent<EnemyMovement>();
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

        OnHealthChanged?.Invoke(_currentHealth, EnemyStatData.MaxHealth);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"적이 죽었습니다.");
        _spawner.RequestRespawn(this);
        _pool.Despawn(EnemyType, this);
    }
}