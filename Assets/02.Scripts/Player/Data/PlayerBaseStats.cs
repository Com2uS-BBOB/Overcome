using UnityEngine;

namespace _02.Scripts.Player.Data
{
    // 플레이어 기본 스탯 (ScriptableObject)
    [CreateAssetMenu(fileName = "PlayerStats", menuName = "Overcome/Player Base Stats")]
    public class PlayerBaseStats : ScriptableObject
    {
        [Header("기본")]
        public float MaxHp = 100f;
        public float AttackDamage = 10f;

        [Header("이동")]
        public float MoveSpeed = 8f;
        public float JumpForce = 10f;

        [Header("질풍참")]
        public float DashDistance = 10f;
        public float DashCooldown = 3f;

        [Header("용검")]
        public float AttackCooldown = 0.5f;

        [Header("크레센트")]
        public float CrescentDamage = 15f;
        public float CrescentSpeed = 20f;
        public float CrescentRange = 30f;
    }
}