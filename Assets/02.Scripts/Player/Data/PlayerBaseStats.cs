using UnityEngine;

namespace _02.Scripts.Player.Data
{
    // 플레이어 기본 스탯 (ScriptableObject)
    [CreateAssetMenu(fileName = "PlayerStats", menuName = "Overcome/Player Base Stats")]
    public class PlayerBaseStats : ScriptableObject
    {
        [Header("기본")]
        public float maxHp = 100f;
        public float attackDamage = 10f;
        public float defense = 5f;

        [Header("이동")]
        public float moveSpeed = 8f;
        public float jumpForce = 10f;

        [Header("질풍참")]
        public float dashDistance = 10f;
        public float dashCooldown = 3f;

        [Header("용검")]
        public float attackCooldown = 0.5f;

        [Header("크레센트")]
        public float crescentDamage = 15f;
        public float crescentSpeed = 20f;
        public float crescentRange = 30f;
        public float crescentCooldown = 1f;
    }
}