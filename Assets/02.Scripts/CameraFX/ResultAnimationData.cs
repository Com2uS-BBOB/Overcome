using System;
using UnityEngine;

namespace _02.Scripts.CameraFX
{
    /// <summary>
    /// 랭크별 결과 애니메이션 매핑 데이터
    /// </summary>
    [CreateAssetMenu(fileName = "ResultAnimationData", menuName = "Game/Result Animation Data")]
    public class ResultAnimationData : ScriptableObject
    {
        [Serializable]
        public class RankAnimation
        {
            [Tooltip("해당 애니메이션을 재생할 랭크들 (예: S, A)")]
            public string[] Grades;

            [Tooltip("Animator Trigger 이름")]
            public string AnimationTrigger;
        }

        [SerializeField] private RankAnimation[] _rankAnimations;
        [SerializeField] private string _defaultAnimation = "Victory_Normal";

        /// <summary>
        /// 랭크에 해당하는 애니메이션 트리거 반환
        /// </summary>
        public string GetAnimationTrigger(string grade)
        {
            if (_rankAnimations == null || _rankAnimations.Length == 0)
                return _defaultAnimation;

            foreach (var rankAnim in _rankAnimations)
            {
                if (rankAnim.Grades == null) continue;

                foreach (var g in rankAnim.Grades)
                {
                    if (string.Equals(g, grade, StringComparison.OrdinalIgnoreCase))
                    {
                        return rankAnim.AnimationTrigger;
                    }
                }
            }

            return _defaultAnimation;
        }
    }
}