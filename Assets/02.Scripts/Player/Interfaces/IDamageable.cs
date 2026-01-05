using System;
using UnityEngine;

namespace _02.Scripts.Player.Interfaces
{
    // 피격 가능 객체 (Player, Enemy)
    public interface IDamageable
    {
        float CurrentHp { get; }
        float MaxHp { get; }
        bool IsDead { get; }
        void TakeDamage(float damage, GameObject attacker = null);
        event Action<float, float> OnHpChanged;
        event Action OnDeath;
    }
}