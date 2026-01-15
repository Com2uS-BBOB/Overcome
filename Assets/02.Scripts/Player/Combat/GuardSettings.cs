using UnityEngine;

namespace _02.Scripts.Player.Combat
{
    /// <summary>
    /// 가드 시스템 설정 (ScriptableObject)
    /// </summary>
    [CreateAssetMenu(fileName = "GuardSettings", menuName = "Overcome/Guard Settings")]
    public class GuardSettings : ScriptableObject
    {
        [Header("Guard Timing")]
        [Tooltip("저스트 가드 판정 윈도우 (가드 시작 후 이 시간 내 피격 시 저스트 가드)")]
        public float JustGuardWindow = 0.3f;

        [Tooltip("가드 해제 후 입력 버퍼 지속 시간")]
        public float GuardInputBuffer = 0.3f;

        [Header("Gauge Cost/Reward")]
        [Tooltip("일반 가드 시 크레센트 소모량")]
        public float NormalGuardCost = 10f;

        [Tooltip("저스트 가드 시 크레센트 회복량")]
        public float JustGuardReward = 20f;

        [Header("Guard Break")]
        [Tooltip("가드 브레이크 경직 시간")]
        public float GuardBreakDuration = 1f;

        [Tooltip("가드 브레이크 시 타임스케일")]
        public float GuardBreakTimeScale = 0.3f;

        [Header("Movement")]
        [Tooltip("가드 중 이동속도 배율")]
        public float GuardMoveSpeedMultiplier = 0.5f;

        [Header("Hit")]
        [Tooltip("피격 경직 시간")]
        public float HitStaggerDuration = 0.3f;

        [Header("Guard Angle")]
        [Tooltip("전방 가드 가능 각도 (반각, 180도 = 90도 반각)")]
        public float FrontGuardAngle = 90f;
    }
}