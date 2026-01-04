using System;
using UnityEngine;

namespace _02.Scripts.Interfaces
{
    /// <summary>
    /// 데미지를 받을 수 있는 객체 인터페이스
    /// Player, Enemy 등이 구현
    /// </summary>
    public interface IDamageable
    {
        float CurrentHp { get; }
        float MaxHp { get; }
        bool IsDead { get; }

        /// <summary>
        /// 데미지 처리
        /// </summary>
        /// <param name="damage">받을 데미지</param>
        /// <param name="attacker">공격자 (null 가능)</param>
        void TakeDamage(float damage, GameObject attacker = null);

        event Action<float, float> OnHpChanged;
        event Action OnDeath;
    }
}