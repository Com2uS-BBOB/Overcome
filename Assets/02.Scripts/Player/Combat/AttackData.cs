using UnityEngine;

namespace _02.Scripts.Player.Combat
{
    /// <summary>
    /// 공격 데이터 (ScriptableObject)
    /// 캔슬 가능한 액션 타입만 관리 (타이밍은 Animation Event로 처리)
    /// </summary>
    [CreateAssetMenu(fileName = "AttackData", menuName = "Combat/AttackData")]
    public class AttackData : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("공격 이름 (Animator State 이름과 매칭)")]
        public string AttackName;

        [Tooltip("콤보 단계 (1, 2, 3...)")]
        public int ComboStep;

        [Header("Allowed Cancels")]
        [Tooltip("이 공격에서 캔슬 가능한 액션 타입들")]
        public ActionType[] AllowedCancels;

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
    }
}