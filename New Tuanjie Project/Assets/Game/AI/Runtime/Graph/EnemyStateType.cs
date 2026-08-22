namespace FMBG.AI
{
    /// <summary>敌人状态类型：运行时行为与 GraphView 编辑器共用。</summary>
    public enum EnemyStateType
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Investigate,
        Return,
        Dead
    }

    /// <summary>状态显示名（编辑器/校验/日志共用）。</summary>
    public static class EnemyStateNames
    {
        public static string Get(EnemyStateType type)
        {
            switch (type)
            {
                case EnemyStateType.Idle: return "Idle 待机";
                case EnemyStateType.Patrol: return "Patrol 巡逻";
                case EnemyStateType.Chase: return "Chase 追击";
                case EnemyStateType.Attack: return "Attack 攻击";
                case EnemyStateType.Investigate: return "Investigate 搜索";
                case EnemyStateType.Return: return "Return 返回";
                case EnemyStateType.Dead: return "Dead 死亡";
                default: return type.ToString();
            }
        }
    }
}
