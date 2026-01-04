using UnityEngine;

namespace _02.Scripts.Player.Data
{
    /// <summary>
    /// 플레이어 기본 스탯 (ScriptableObject)
    /// 에디터에서 스탯 프리셋 관리
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerStats", menuName = "Overcome/Player Base Stats")]
    public class PlayerBaseStats : ScriptableObject
    {
        [Header("기본 스탯")]
        [Tooltip("최대 체력")]
        public float maxHp = 100f;

        [Tooltip("공격력")]
        public float attackDamage = 10f;

        [Tooltip("방어력")]
        public float defense = 5f;

        [Header("이동 스탯")]
        [Tooltip("이동 속도")]
        public float moveSpeed = 8f;

        [Tooltip("점프력")]
        public float jumpForce = 10f;

        [Header("스킬 스탯")]
        [Tooltip("질풍참 이동 거리")]
        public float dashDistance = 10f;

        [Tooltip("질풍참 쿨타임")]
        public float dashCooldown = 3f;

        [Tooltip("기본 공격 쿨타임")]
        public float attackCooldown = 0.5f;
    }
}