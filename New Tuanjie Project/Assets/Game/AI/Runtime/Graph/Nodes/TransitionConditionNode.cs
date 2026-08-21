using UnityEngine;
using XNode;

namespace FMBG.AI
{
    /// <summary>
    /// 可视化切换条件节点：状态节点的输出端口连接到此节点，
    /// 条件满足时切换到 target 端口连接的目标状态。
    /// </summary>
    [CreateNodeMenu("Enemy AI/Conditions/Transition Condition")]
    public sealed class TransitionConditionNode : Node
    {
                // 输入端口类型必须与状态节点输出端口类型（TransitionConditionNode）匹配，
        // 否则 xNode 编辑器会视为非法连接导致节点无法交互。
        [Input] public TransitionConditionNode from;

        [Tooltip("切换条件配置（类型与参数）。")]
        public TransitionCondition condition = new();

        [Output] public EnemyStateNode target;

        public bool Evaluate(EnemyContext context)
        {
            return condition != null && condition.Evaluate(context);
        }

        public EnemyStateNode GetTargetState()
        {
            NodePort port = GetOutputPort(nameof(target));
            if (port == null || port.ConnectionCount == 0)
            {
                return null;
            }

            return port.GetConnection(0).node as EnemyStateNode;
        }

        public string Describe()
        {
            return condition != null ? condition.Describe() : "未配置";
        }
    }
}
