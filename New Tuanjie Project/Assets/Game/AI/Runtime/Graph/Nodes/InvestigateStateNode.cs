using UnityEngine;
using XNode;

namespace FMBG.AI
{
    [CreateNodeMenu("Enemy AI/States/Investigate")]
    public sealed class InvestigateStateNode : EnemyStateNode
    {
        [Output] public TransitionConditionNode chase;
        [Output] public TransitionConditionNode returnNode;

        [Min(0f)] public float investigateDuration = 3f;

        public override void Enter(EnemyContext context)
        {
            context.Blackboard.StateTime = 0f;
            context.Motor.SetPatrolSpeed();
            context.Motor.Resume();

            if (context.Blackboard.HasLastKnownPosition)
            {
                context.Motor.MoveTo(context.Blackboard.LastKnownPosition);
            }
        }

        public override void Tick(EnemyContext context, float deltaTime)
        {
            // StateTime 由 EnemyStateMachineRunner 统一推进（此处不再重复累加）

            // 到达搜索点后原地转向搜索
            if (context.Motor.ReachedDestination())
            {
                context.Motor.Stop();
                if (context.Perception.Target != null)
                {
                    context.Motor.FaceTowards(context.Perception.Target.position, deltaTime);
                }
            }
        }

        public override void Exit(EnemyContext context)
        {
            context.Motor.Stop();
        }
    }
}
