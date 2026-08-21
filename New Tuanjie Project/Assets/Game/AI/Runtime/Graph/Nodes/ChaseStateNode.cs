using UnityEngine;
using XNode;

namespace FMBG.AI
{
    [CreateNodeMenu("Enemy AI/States/Chase")]
    public sealed class ChaseStateNode : EnemyStateNode
    {
        [Output] public EnemyStateNode attack;
        [Output] public EnemyStateNode investigate;

        [Min(0.02f)] public float repathInterval = 0.15f;
        public float stoppingDistanceTolerance = 0.1f;

        public override void Enter(EnemyContext context)
        {
            context.Blackboard.RepathTimer = 0f;
            context.Motor.SetChaseSpeed();
            context.Motor.Resume();
        }

        public override void Tick(EnemyContext context, float deltaTime)
        {
            Transform target = context.Perception.Target;
            if (target == null)
            {
                return;
            }

            context.Blackboard.LastKnownPosition = target.position;
            context.Blackboard.HasLastKnownPosition = true;

            context.Blackboard.RepathTimer -= deltaTime;
            if (context.Blackboard.RepathTimer > 0f)
            {
                return;
            }

            context.Blackboard.RepathTimer = repathInterval;

            float stoppingDistance = context.Combat.CurrentWeapon != null
                ? context.Combat.CurrentWeapon.MaxAttackRange - stoppingDistanceTolerance
                : 0.5f;

            context.Motor.SetStoppingDistance(Mathf.Max(0.1f, stoppingDistance));
            context.Motor.MoveTo(target.position);
        }

        public override EnemyStateNode EvaluateTransition(EnemyContext context)
        {
            if (!context.Perception.CanSeeTarget)
            {
                return GetConnectedNode<EnemyStateNode>(nameof(investigate));
            }

            Transform target = context.Perception.Target;
            if (target != null && context.Combat.IsTargetInAttackRange(target))
            {
                return GetConnectedNode<EnemyStateNode>(nameof(attack));
            }

            return null;
        }

        public override void Exit(EnemyContext context)
        {
            context.Motor.Stop();
        }
    }
}
