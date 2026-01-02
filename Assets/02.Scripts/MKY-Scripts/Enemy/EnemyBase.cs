using System;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("스탯")]
    public EnemyStatData EnemyStatData;

    protected float _currentHealth;
    // protected EnemyMovement _movement;

    public EEnemyType EnemyType { get; private set; }

    private EnemyPool _pool;

    public event Action<float, float> OnHealthChanged;

    public void SetPool(EnemyPool pool)
    {
        _pool = pool;
    }

    public void SetEnemyType(EEnemyType type)
    {
        EnemyType = type;
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
        _pool.Despawn(EnemyType, this);
    }
}