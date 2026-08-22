using FMBG.Combat;
using FMBG.Skills;
using UnityEngine;

namespace FMBG.AI
{
    /// <summary>攻击：转向目标并通过技能控制器/武器释放攻击；施法中不切换状态。</summary>
    public sealed class AttackState : EnemyState
    {
        public AttackState(EnemyStateNodeData data) : base(data)
        {
        }

        public override void Enter(EnemyContext context)
        {
            context.Motor.Stop();
        }

        public override void Tick(EnemyContext context, float deltaTime)
        {
            Transform target = context.Perception.Target;
            if (target == null)
            {
                return;
            }

            context.Motor.FaceTowards(target.position, deltaTime);

            // 优先使用技能控制器（Slate 技能），否则回退到武器基础攻击
            if (context.SkillController != null && context.SkillController.IsCasting)
            {
                return;
            }

            if (context.SkillController != null && context.SkillSelector != null)
            {
                WeaponConfig weapon = context.Combat.CurrentWeaponConfig;
                SkillConfig skill = context.SkillSelector.SelectSkill(weapon, target);

                if (skill != null)
                {
                    context.SkillController.TryCast(
                        skill,
                        new SkillCastRequest(target.position, target));
                    return;
                }
            }

            if (context.Combat.CurrentWeapon != null &&
                !context.Combat.CurrentWeapon.IsAttacking)
            {
                context.Combat.TryAttack(target.position, target);
            }
        }

        /// <summary>技能施法过程中不切换状态（全局守卫）。</summary>
        public override EnemyState EvaluateTransitions(EnemyContext context)
        {
            if (context.SkillController != null && context.SkillController.IsCasting)
            {
                return null;
            }

            return base.EvaluateTransitions(context);
        }

        public override void Exit(EnemyContext context)
        {
            context.Combat.CancelAttack();

            if (context.SkillController != null)
            {
                context.SkillController.Interrupt();
            }
        }
    }
}
