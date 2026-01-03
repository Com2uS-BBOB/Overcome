using System;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("스탯")]
    public EnemyStatData EnemyStatData;

    [Header("스폰 높이")]
    [SerializeField] private float _spawnHeight = 1f;

    protected float _currentHealth;
    // protected EnemyMovement _movement;

    public EEnemyType EnemyType { get; private set; }

    private EnemyPool _pool;
    private EnemySpawner _spawner;

    private Vector3 _spawnOffset;

    public event Action<float, float> OnHealthChanged;


    protected virtual void Awake()
    {
        // 프리팹 상태에서의 로컬 위치 기억
        _spawnOffset = transform.localPosition;
    }

    public Vector3 GetSpawnOffset()
    {
        return Vector3.up * _spawnHeight;
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

    protected virtual void OnEnable()
    {
        // _movement = GetComponent<EnemyMovement>();
        _currentHealth = EnemyStatData.MaxHealth;
        OnHealthChanged?.Invoke(_currentHealth, EnemyStatData.MaxHealth);
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