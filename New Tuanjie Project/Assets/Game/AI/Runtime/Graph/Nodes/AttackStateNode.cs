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

            if (context.Combat.CurrentWeapon != null &&
                !context.Combat.CurrentWeapon.IsAttacking)
            {
                context.Combat.TryAttack(target.position, target);
            }
        }

        public override EnemyStateNode EvaluateTransition(EnemyContext context)
        {
            if (!context.Perception.CanSeeTarget)
            {
                return GetConnectedNode<EnemyStateNode>(nameof(investigate));
            }

            Transform target = context.Perception.Target;
            if (target == null)
            {
                return GetConnectedNode<EnemyStateNode>(nameof(investigate));
            }

            bool outsideRange = !context.Combat.IsTargetInAttackRange(target, exitRangeTolerance);
            if (outsideRange)
            {
                return GetConnectedNode<EnemyStateNode>(nameof(chase));
            }

            return null;
        }

        public override void Exit(EnemyContext context)
        {
            context.Combat.CancelAttack();
        }
    }
}
