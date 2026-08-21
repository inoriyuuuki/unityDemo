using UnityEngine;
using XNode;

namespace FMBG.AI
{
    [CreateNodeMenu("Enemy AI/States/Patrol")]
    public sealed class PatrolStateNode : EnemyStateNode
    {
        [Output] public TransitionConditionNode idle;
        [Output] public TransitionConditionNode chase;

        public override void Enter(EnemyContext context)
        {
            context.Motor.SetPatrolSpeed();
            context.Motor.Resume();

            if (!context.Blackboard.HasPatrolPoints || context.Blackboard.PatrolPoints.Length == 0)
            {
                context.Blackboard.PatrolPoints = context.Actor.GetPatrolPoints();
                context.Blackboard.HasPatrolPoints = true;
            }
        }

        public override void Tick(EnemyContext context, float deltaTime)
        {
            if (!context.Blackboard.HasPatrolPoints || context.Blackboard.PatrolPoints.Length == 0)
            {
                return;
            }

            int index = context.Blackboard.PatrolPointIndex;
            context.Motor.MoveTo(context.Blackboard.PatrolPoints[index]);

            if (context.Motor.ReachedDestination())
            {
                context.Blackboard.PatrolPointIndex =
                    (context.Blackboard.PatrolPointIndex + 1) % context.Blackboard.PatrolPoints.Length;
            }
        }

        public override void Exit(EnemyContext context)
        {
            context.Motor.Stop();
        }
    }
}
