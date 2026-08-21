namespace FMBG.AI
{
    /// <summary>敌人运行时数据，xNode 节点不保存运行时状态。</summary>
    public sealed class EnemyBlackboard
    {
        public float StateTime;
        public float RepathTimer;

        public int PatrolPointIndex;
        public UnityEngine.Vector3[] PatrolPoints;
        public bool HasPatrolPoints;

        public UnityEngine.Vector3 LastKnownPosition;
        public bool HasLastKnownPosition;
    }
}
