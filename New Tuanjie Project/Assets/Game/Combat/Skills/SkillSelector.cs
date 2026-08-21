using FMBG.Combat;
using UnityEngine;

namespace FMBG.Skills
{
    /// <summary>敌人 AI 技能选择器：从当前武器挑选可用技能。</summary>
    public sealed class SkillSelector : MonoBehaviour
    {
        [SerializeField] private CharacterSkillController skillController;

        public SkillConfig SelectSkill(WeaponConfig weapon, Transform target)
        {
            if (weapon == null)
            {
                return null;
            }

            // 优先普通攻击
            SkillConfig basic = weapon.BasicAttack;
            if (basic != null && IsUsable(basic, target))
            {
                return basic;
            }

            // 其次按顺序选第一个可用的技能
            if (weapon.Skills != null)
            {
                foreach (SkillConfig skill in weapon.Skills)
                {
                    if (IsUsable(skill, target))
                    {
                        return skill;
                    }
                }
            }

            return null;
        }

        public bool HasUsableSkillInRange()
        {
            return true; // 由 SkillController.TryCast 做精确范围校验
        }

        private bool IsUsable(SkillConfig skill, Transform target)
        {
            if (skillController != null && skillController.IsOnCooldown(skill))
            {
                return false;
            }

            return true;
        }
    }
}
