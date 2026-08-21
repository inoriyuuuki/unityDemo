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

        public override void Exit(EnemyContext context)
        {
            context.Motor.Stop();
        }
    }
}
