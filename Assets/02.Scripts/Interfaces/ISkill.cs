using System;

namespace _02.Scripts.Interfaces
{
    /// <summary>
    /// 스킬 인터페이스
    /// 용검, 크레센트, 질풍참 등이 구현
    /// </summary>
    public interface ISkill
    {
        string SkillName { get; }
        float Cooldown { get; }
        bool CanUse { get; }

        void Use();

        event Action OnSkillUsed;
    }
}