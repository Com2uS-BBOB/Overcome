using System;

namespace _02.Scripts.Interfaces
{
    /// <summary>
    /// 히트 감지 인터페이스
    /// MeleeHitbox, CrescentProjectile 등이 구현
    /// </summary>
    public interface IHitDetector
    {
        event Action<IDamageable, float> OnHit;

        void EnableHitDetection(float damage);
        void DisableHitDetection();
    }
}