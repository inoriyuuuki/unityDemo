using FMBG.Combat;

namespace FMBG.AI
{
    /// <summary>敌人状态机运行上下文，节点通过它访问组件与配置。</summary>
    public sealed class EnemyContext
    {
        public EnemyActor Actor { get; }
        public EnemyConfig Config { get; }

        public EnemyPerception Perception { get; }
        public EnemyMotor Motor { get; }
        public CharacterCombat Combat { get; }
        public Health Health { get; }

        public EnemyBlackboard Blackboard { get; }

        public EnemyContext(
            EnemyActor actor,
            EnemyConfig config,
            EnemyPerception perception,
            EnemyMotor motor,
            CharacterCombat combat,
            Health health)
        {
            Actor = actor;
            Config = config;
            Perception = perception;
            Motor = motor;
            Combat = combat;
            Health = health;
            Blackboard = new EnemyBlackboard();
        }
    }
}
