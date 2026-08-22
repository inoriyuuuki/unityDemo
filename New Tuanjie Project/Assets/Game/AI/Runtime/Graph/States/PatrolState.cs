namespace FMBG.AI
{
    /// <summary>巡逻：依次访问巡逻点，到达后推进到下一个（连续巡逻）。</summary>
    public sealed class PatrolState : EnemyState
    {
        public PatrolState(EnemyStateNodeData data) : base(data)
        {
        }

        public override void Enter(EnemyContext context)
        {
            context.Motor.SetPatrolSpeed();
            context.Motor.Resume();

            if (!context.Blackboard.HasPatrolPoints || context.Blackboard.PatrolPoints.Length == 0)
            {
                context.Blackboard.PatrolPoints = context.Actor.GetPatrolPoints();
                context.Blackboard.HasPatrolPoints = true;
            }

            // 立即设置当前巡逻目标，避免第一帧被"到达目的地"误判
            if (context.Blackboard.HasPatrolPoints && context.Blackboard.PatrolPoints.Length > 0)
            {
                context.Motor.MoveTo(context.Blackboard.PatrolPoints[context.Blackboard.PatrolPointIndex]);
            }
        }

        /// <summary>
        /// 到达当前巡逻点后立即推进到下一个巡逻点并继续移动（连续巡逻），
        /// 避免"到达目的地"条件在 Tick 推进 index 之前就把状态切回 Idle。
        /// </summary>
        public override EnemyState EvaluateTransitions(EnemyContext context)
        {
            if (context.Blackboard.HasPatrolPoints &&
                context.Blackboard.PatrolPoints.Length > 0 &&
                context.Motor.ReachedDestination())
            {
                context.Blackboard.PatrolPointIndex =
                    (context.Blackboard.PatrolPointIndex + 1) % context.Blackboard.PatrolPoints.Length;

                // 立即前往下一个巡逻点；"到达目的地"条件此刻不再满足，避免被切回 Idle
                context.Motor.MoveTo(context.Blackboard.PatrolPoints[context.Blackboard.PatrolPointIndex]);
            }

            // 其余转换（如发现目标 → Chase）仍由配置条件驱动
            return base.EvaluateTransitions(context);
        }

        public override void Tick(EnemyContext context, float deltaTime)
        {
            if (!context.Blackboard.HasPatrolPoints || context.Blackboard.PatrolPoints.Length == 0)
            {
                return;
            }

            context.Motor.MoveTo(context.Blackboard.PatrolPoints[context.Blackboard.PatrolPointIndex]);
        }

        public override void Exit(EnemyContext context)
        {
            context.Motor.Stop();
        }
    }
}
