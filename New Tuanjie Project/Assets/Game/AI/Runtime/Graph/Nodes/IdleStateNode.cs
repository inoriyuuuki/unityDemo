using UnityEngine;
using XNode;

namespace FMBG.AI
{
    [CreateNodeMenu("Enemy AI/States/Idle")]
    public sealed class IdleStateNode : EnemyStateNode
    {
        [Output] public TransitionConditionNode patrol;
        [Output] public TransitionConditionNode chase;

        [Min(0f)] public float idleDuration = 2f;

        public override void Enter(EnemyContext context)
        {
            context.Blackboard.StateTime = 0f;
            context.Motor.Stop();
        }

        // 注意：StateTime 由 EnemyStateMachineRunner 统一推进，节点无需（也不应）自行累加，
        // 否则计时条件（TimerElapsed）会以双倍速度触发。
        public override void Exit(EnemyContext context)
        {
        }
    }
}
