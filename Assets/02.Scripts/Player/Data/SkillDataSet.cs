using UnityEngine;

namespace _02.Scripts.Player.Data
{
    /// <summary>
    /// 스킬 데이터 세트 (ScriptableObject)
    /// 지상/공중 SkillData를 페어로 관리
    /// </summary>
    [CreateAssetMenu(fileName = "SkillDataSet", menuName = "Combat/SkillDataSet")]
    public class SkillDataSet : ScriptableObject
    {
        [Header("Skill Set Identity")]
        [Tooltip("스킬 세트 이름 (DragonSword, Crescent 등)")]
        public string SetName;

        [Header("Ground/Air Data")]
        [Tooltip("지상 스킬 데이터")]
        public SkillData GroundSkill;

        [Tooltip("공중 스킬 데이터")]
        public SkillData AirSkill;

        // === Methods ===

        /// <summary>
        /// 현재 상황에 맞는 SkillData 반환
        /// </summary>
        /// <param name="isGrounded">현재 지상인지 여부</param>
        public SkillData GetSkillData(bool isGrounded)
        {
            return isGrounded ? GroundSkill : AirSkill;
        }

        /// <summary>
        /// 콤보 시작 시점 기준 SkillData 반환
        /// 콤보 중간에 착지/점프해도 원래 설정 유지
        /// </summary>
        /// <param name="wasGroundedOnStart">콤보 시작 시 지상이었는지 여부</param>
        public SkillData GetSkillDataByComboStart(bool wasGroundedOnStart)
        {
            return wasGroundedOnStart ? GroundSkill : AirSkill;
        }

        /// <summary>
        /// 지상 스킬의 최대 콤보 수
        /// </summary>
        public int GroundMaxCombo => GroundSkill?.MaxCombo ?? 0;

        /// <summary>
        /// 공중 스킬의 최대 콤보 수
        /// </summary>
        public int AirMaxCombo => AirSkill?.MaxCombo ?? 0;
    }
}