using UnityEngine;
using XNode;

namespace FMBG.AI
{
    [CreateNodeMenu("Enemy AI/States/Idle")]
    public sealed class IdleStateNode : EnemyStateNode
    {
        [Output] public EnemyStateNode patrol;
        [Output] public EnemyStateNode chase;

        [Min(0f)] public float idleDuration = 2f;

        public override void Enter(EnemyContext context)
        {
            context.Blackboard.StateTime = 0f;
            context.Motor.Stop();
        }

        public override void Tick(EnemyContext context, float deltaTime)
        {
            context.Blackboard.StateTime += deltaTime;
        }

        public override void Exit(EnemyContext context)
        {
        }
    }
}
