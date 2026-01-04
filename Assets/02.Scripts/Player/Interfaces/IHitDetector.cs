using System;

namespace _02.Scripts.Interfaces
{
    // 히트 감지 (MeleeHitbox, CrescentProjectile)
    public interface IHitDetector
    {
        event Action<IDamageable, float> OnHit;
        void EnableHitDetection(float damage);
        void DisableHitDetection();
    }
}