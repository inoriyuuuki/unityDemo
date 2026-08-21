using System.Collections.Generic;
using FMBG.Combat;
using UnityEngine;

namespace FMBG.Skills
{
    /// <summary>每次施法独立上下文，Slate 时间轴不能把目标写进资源。</summary>
    public sealed class SkillExecutionContext
    {
        public CharacterSkillController Caster { get; }
        public CharacterCombat Combat { get; }
        public SkillConfig Skill { get; }
        public WeaponConfig Weapon { get; }
        public Transform Target { get; }
        public Vector3 TargetPosition { get; }

        public HashSet<IDamageable> HitTargets { get; } = new();

        public SkillExecutionContext(
            CharacterSkillController caster,
            CharacterCombat combat,
            SkillConfig skill,
            WeaponConfig weapon,
            Transform target,
            Vector3 targetPosition)
        {
            Caster = caster;
            Combat = combat;
            Skill = skill;
            Weapon = weapon;
            Target = target;
            TargetPosition = targetPosition;
        }

        public float CalculateDamage()
        {
            float baseDamage = Weapon != null ? Weapon.Damage : 10f;
            float multiplier = Skill != null ? Skill.DamageMultiplier : 1f;
            return baseDamage * multiplier;
        }
    }
}
