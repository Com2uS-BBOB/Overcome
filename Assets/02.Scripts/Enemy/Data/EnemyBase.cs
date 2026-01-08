using UnityEngine;
using System;
using _02.Scripts.Player.Interfaces;

public abstract class EnemyBase : MonoBehaviour, IDamageable
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

    public float CurrentHp => _currentHealth;
    public float MaxHp => EnemyStatData.MaxHealth;
    public bool IsDead => _currentHealth <= 0;

    public event Action<float, float> OnHpChanged;     // 적의 체력 변화 이벤트
    public event Action<float> OnEnemyHit;             // 적이 맞았을 때 이벤트
    public event Action<EnemyStatData> OnEnemyKilled;  // 적이 죽었을 때  보상용 이벤트
    public event Action OnDeath;                       // 적이 죽었을 때 이벤트

    protected virtual void OnEnable()
    {
        _currentHealth = EnemyStatData.MaxHealth;
        OnHpChanged?.Invoke(_currentHealth, MaxHp);
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


    public virtual void TakeDamage(float damage, GameObject attacker = null)
    {
        if (IsDead) return;

        _currentHealth -= damage;
        _currentHealth = Mathf.Max(_currentHealth, 0);

        OnEnemyHit?.Invoke(damage);  // 적이 맞았을 때 이벤트 호출
        OnHpChanged?.Invoke(_currentHealth, MaxHp);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"적이 죽었습니다.");

        OnEnemyKilled?.Invoke(EnemyStatData);  // EnemyReward에 보상 제공 알림
        OnDeath?.Invoke();

        _spawner.RequestRespawn(this);
        _pool.Despawn(EnemyType, this);
    }
}