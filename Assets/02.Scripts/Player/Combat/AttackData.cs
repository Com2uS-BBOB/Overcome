using UnityEngine;

namespace _02.Scripts.Player.Combat
{
    /// <summary>
    /// 공격 데이터 (ScriptableObject)
    /// Inspector에서 캔슬 윈도우, 허용 캔슬, 히트박스 타이밍 설정
    /// </summary>
    [CreateAssetMenu(fileName = "AttackData", menuName = "Combat/AttackData")]
    public class AttackData : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("공격 이름 (Animator State 이름과 매칭)")]
        public string AttackName;

        [Tooltip("콤보 단계 (1, 2, 3...)")]
        public int ComboStep;

        [Header("Cancel Window (Normalized Time 0~1)")]
        [Tooltip("캔슬 가능 시작 시점 (0~1, 애니메이션 정규화 시간)")]
        [Range(0f, 1f)]
        public float CancelWindowStart = 0.4f;

        [Tooltip("캔슬 가능 종료 시점 (0~1, 애니메이션 정규화 시간)")]
        [Range(0f, 1f)]
        public float CancelWindowEnd = 0.9f;

        [Header("Allowed Cancels")]
        [Tooltip("이 공격에서 캔슬 가능한 액션 타입들")]
        public ActionType[] AllowedCancels;

        [Header("Hitbox Timing (Normalized Time 0~1)")]
        [Tooltip("히트박스 활성화 시작 시점")]
        [Range(0f, 1f)]
        public float HitboxStart = 0.15f;

        [Tooltip("히트박스 비활성화 시점")]
        [Range(0f, 1f)]
        public float HitboxEnd = 0.45f;

        [Header("Duration")]
        [Tooltip("공격 전체 지속 시간 (초)")]
        public float Duration = 0.8f;

        /// <summary>
        /// 특정 액션으로 캔슬 가능한지 확인
        /// </summary>
        public bool CanCancelInto(ActionType targetAction)
        {
            if (AllowedCancels == null || AllowedCancels.Length == 0)
                return false;

            foreach (var allowed in AllowedCancels)
            {
                if (allowed == ActionType.Any)
                    return true;
                if (allowed == targetAction)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 정규화 시간이 캔슬 윈도우 내에 있는지 확인
        /// </summary>
        public bool IsInCancelWindow(float normalizedTime)
        {
            return normalizedTime >= CancelWindowStart && normalizedTime <= CancelWindowEnd;
        }

        /// <summary>
        /// 정규화 시간이 히트박스 활성 구간 내에 있는지 확인
        /// </summary>
        public bool IsInHitboxWindow(float normalizedTime)
        {
            return normalizedTime >= HitboxStart && normalizedTime <= HitboxEnd;
        }
    }
}