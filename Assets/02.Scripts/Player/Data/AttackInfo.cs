using UnityEngine;

namespace _02.Scripts.Player.Data
{
    /// <summary>
    /// 넉백 레벨
    /// </summary>
    public enum KnockbackLevel
    {
        None = 0,   // 넉백 없음
        Light = 1,  // Lv.1 - 일반 공격, 약한 넉백
        Heavy = 2   // Lv.2 - 차지 공격, 강한 넉백
    }

    /// <summary>
    /// 공격 정보 (적 공격에서 플레이어 피격 시스템으로 전달)
    /// </summary>
    [System.Serializable]
    public struct AttackInfo
    {
        public float Damage;
        public KnockbackLevel KnockbackLevel;
        public Vector3 Direction;
        public GameObject Attacker;
        public float KnockbackDistance;
        public float KnockbackDuration;

        // 기본값 상수
        private const float LightKnockbackDistance = 2f;
        private const float HeavyKnockbackDistance = 4f;
        private const float LightKnockbackDuration = 0.2f;
        private const float HeavyKnockbackDuration = 0.3f;

        public AttackInfo(float damage, KnockbackLevel level, Vector3 direction, GameObject attacker = null)
        {
            Damage = damage;
            KnockbackLevel = level;
            Direction = direction.sqrMagnitude > 0.01f ? direction.normalized : Vector3.zero;
            Attacker = attacker;

            // 레벨에 따른 기본값 설정
            KnockbackDistance = level == KnockbackLevel.Heavy ? HeavyKnockbackDistance : LightKnockbackDistance;
            KnockbackDuration = level == KnockbackLevel.Heavy ? HeavyKnockbackDuration : LightKnockbackDuration;
        }

        public AttackInfo(float damage, KnockbackLevel level, Vector3 direction,
            float knockbackDistance, float knockbackDuration, GameObject attacker = null)
        {
            Damage = damage;
            KnockbackLevel = level;
            Direction = direction.sqrMagnitude > 0.01f ? direction.normalized : Vector3.zero;
            Attacker = attacker;
            KnockbackDistance = knockbackDistance;
            KnockbackDuration = knockbackDuration;
        }
    }
}