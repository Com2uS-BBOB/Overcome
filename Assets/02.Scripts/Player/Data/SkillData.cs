using UnityEngine;
using _02.Scripts.Player.Combat;

namespace _02.Scripts.Player.Data
{
    /// <summary>
    /// 스킬 데이터 (ScriptableObject)
    /// 콤보 체인, 물리 설정 등을 관리 (타이밍은 Animation Event로 처리)
    /// 지상/공중 스킬을 별도의 에셋으로 분리하여 데이터 드리븐 설계 구현
    /// </summary>
    [CreateAssetMenu(fileName = "SkillData", menuName = "Combat/SkillData")]
    public class SkillData : ScriptableObject
    {
        [Header("Skill Identity")]
        [Tooltip("스킬 이름 (DragonSword, Crescent 등)")]
        public string SkillName;

        [Tooltip("액션 타입 (Attack, Skill, DashAttack)")]
        public ActionType ActionType;

        [Header("Combo Chain")]
        [Tooltip("콤보 순서대로 배열 (1타, 2타, 3타...)")]
        public AttackData[] ComboChain;

        [Header("Combo Settings")]
        [Tooltip("콤보 유예 시간 (초)")]
        public float ComboGraceTime = 0.1f;

        [Header("Combat Properties")]
        [Tooltip("Root Motion 사용 여부")]
        public bool UseRootMotion = true;

        [Tooltip("중력 스케일 (공중: 0.3~0.8, 지상: 1.0)")]
        [Range(0f, 2f)]
        public float GravityScale = 1f;

        [Tooltip("콤보 시 상승력 (공중 전용)")]
        public float ComboLiftForce = 0f;

        [Tooltip("공격 중 이동 속도 배율")]
        [Range(0f, 1f)]
        public float MoveSpeedMultiplier = 0.2f;

        [Header("Resource Cost")]
        [Tooltip("게이지 필요 여부 (크레센트 등)")]
        public bool RequiresGauge = false;

        [Tooltip("콤보당 게이지 소모량")]
        public int GaugeCostPerCombo = 1;

        // === Properties ===

        /// <summary>
        /// 최대 콤보 수 (ComboChain 배열 길이)
        /// </summary>
        public int MaxCombo => ComboChain?.Length ?? 0;

        // === Methods ===

        /// <summary>
        /// 특정 콤보 단계의 AttackData 반환
        /// </summary>
        /// <param name="step">콤보 단계 (1-indexed)</param>
        public AttackData GetComboData(int step)
        {
            if (ComboChain == null || step < 1 || step > ComboChain.Length)
                return null;
            return ComboChain[step - 1];
        }

        /// <summary>
        /// 특정 콤보 단계에서 특정 액션으로 캔슬 가능한지 확인
        /// </summary>
        public bool CanCancelInto(int comboStep, ActionType targetAction)
        {
            var data = GetComboData(comboStep);
            return data?.CanCancelInto(targetAction) ?? false;
        }
    }
}