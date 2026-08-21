using UnityEngine;
using XNode;

namespace FMBG.AI
{
    [CreateNodeMenu("Enemy AI/States/Return")]
    public sealed class ReturnStateNode : EnemyStateNode
    {
        [Output] public EnemyStateNode patrol;
        [Output] public EnemyStateNode chase;

        public override void Enter(EnemyContext context)
        {
            context.Motor.SetPatrolSpeed();
            context.Motor.Resume();

            Vector3 returnTarget = context.Actor.GetReturnPosition();
            context.Motor.MoveTo(returnTarget);
        }

        public override EnemyStateNode EvaluateTransition(EnemyContext context)
        {
            if (context.Perception.CanSeeTarget)
            {
                return GetConnectedNode<EnemyStateNode>(nameof(chase));
            }

            if (context.Motor.ReachedDestination())
            {
                return GetConnectedNode<EnemyStateNode>(nameof(patrol));
            }

            return null;
        }

        public override void Exit(EnemyContext context)
        {
            context.Motor.Stop();
        }
    }
}
