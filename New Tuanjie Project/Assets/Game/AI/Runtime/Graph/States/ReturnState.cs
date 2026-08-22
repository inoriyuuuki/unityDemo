using UnityEngine;

namespace FMBG.AI
{
    /// <summary>返回：返回出生/最近巡逻区域，途中仍可发现目标。</summary>
    public sealed class ReturnState : EnemyState
    {
        public ReturnState(EnemyStateNodeData data) : base(data)
        {
        }

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
