using System;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("스탯")]
    public EnemyStatData EnemyStatData;

    protected float _currentHp;
    // protected EnemyMovement _movement;

    public event Action<float, float> OnHealthChanged;

    protected virtual void Awake()
    {
        // _movement = GetComponent<EnemyMovement>();
        _currentHp = EnemyStatData.MaxHealth;

        OnHealthChanged?.Invoke(_currentHp, EnemyStatData.MaxHealth);
    }

    public virtual void TakeDamage(float damage)
    {
        _currentHp -= damage;
        _currentHp = Mathf.Max(_currentHp, 0);

        OnHealthChanged?.Invoke(_currentHp, EnemyStatData.MaxHealth);

        if (_currentHp <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"적이 죽었습니다.");
        Destroy(gameObject);
    }
}