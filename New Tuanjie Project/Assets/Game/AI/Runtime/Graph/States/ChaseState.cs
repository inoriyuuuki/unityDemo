using UnityEngine;

namespace FMBG.AI
{
    /// <summary>追击：定期重寻路，进入武器范围由条件转换驱动。</summary>
    public sealed class ChaseState : EnemyState
    {
        private readonly float repathInterval;
        private readonly float stoppingDistanceTolerance;

        public ChaseState(EnemyStateNodeData data) : base(data)
        {
            repathInterval = data != null ? data.RepathInterval : 0.15f;
            stoppingDistanceTolerance = data != null ? data.StoppingDistanceTolerance : 0.1f;
        }

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

        public override void Exit(EnemyContext context)
        {
            context.Motor.Stop();
        }
    }
}
