namespace FMBG.AI
{
    /// <summary>待机：停留并观察，由出向转换（计时/发现目标）驱动切换。</summary>
    public sealed class IdleState : EnemyState
    {
        public IdleState(EnemyStateNodeData data) : base(data)
        {
        }

        public override void Enter(EnemyContext context)
        {
            context.Blackboard.StateTime = 0f;
            context.Motor.Stop();
        }
    }
}
