using UnityEngine;

namespace FMBG.AI
{
    /// <summary>搜索：前往最后发现位置，到达后原地转向搜索。</summary>
    public sealed class InvestigateState : EnemyState
    {
        public InvestigateState(EnemyStateNodeData data) : base(data)
        {
        }

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
