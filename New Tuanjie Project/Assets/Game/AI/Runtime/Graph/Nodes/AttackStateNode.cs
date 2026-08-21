using UnityEngine;
using XNode;

namespace FMBG.AI
{
    [CreateNodeMenu("Enemy AI/States/Attack")]
    public sealed class AttackStateNode : EnemyStateNode
    {
        [Output] public EnemyStateNode chase;
        [Output] public EnemyStateNode investigate;

        [Min(0f)] public float exitRangeTolerance = 0.4f;

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
                var weapon = context.Combat.CurrentWeaponConfig;
                FMBG.Skills.SkillConfig skill =
                    context.SkillSelector.SelectSkill(weapon, target);

                if (skill != null)
                {
                    context.SkillController.TryCast(
                        skill,
                        new FMBG.Skills.SkillCastRequest(target.position, target));
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
        public override EnemyStateNode EvaluateTransition(EnemyContext context)
        {
            if (context.SkillController != null && context.SkillController.IsCasting)
            {
                return null;
            }

            return base.EvaluateTransition(context);
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
