using System;

namespace _02.Scripts.Interfaces
{
    // 스킬 (용검, 크레센트, 질풍참)
    public interface ISkill
    {
        string SkillName { get; }
        float Cooldown { get; }
        bool CanUse { get; }
        void Use();
        event Action OnSkillUsed;
    }
}