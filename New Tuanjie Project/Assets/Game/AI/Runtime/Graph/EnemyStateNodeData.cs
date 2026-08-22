using UnityEngine;

namespace FMBG.AI
{
    /// <summary>状态节点数据：纯序列化配置（GraphView 编辑、运行时编译）。</summary>
    [System.Serializable]
    public sealed class EnemyStateNodeData
    {
        [SerializeField] private string id;
        [SerializeField] private EnemyStateType type;
        [SerializeField] private Vector2 position;

        [Header("Chase")]
        [SerializeField, Min(0.02f)] private float repathInterval = 0.15f;
        [SerializeField, Min(0f)] private float stoppingDistanceTolerance = 0.1f;

        public string Id => id;
        public EnemyStateType Type => type;
        public Vector2 Position { get => position; set => position = value; }
        public float RepathInterval { get => repathInterval; set => repathInterval = Mathf.Max(0.02f, value); }
        public float StoppingDistanceTolerance { get => stoppingDistanceTolerance; set => stoppingDistanceTolerance = Mathf.Max(0f, value); }

        public EnemyStateNodeData()
        {
        }

        public EnemyStateNodeData(string id, EnemyStateType type, Vector2 position)
        {
            this.id = id;
            this.type = type;
            this.position = position;
        }
    }
}
