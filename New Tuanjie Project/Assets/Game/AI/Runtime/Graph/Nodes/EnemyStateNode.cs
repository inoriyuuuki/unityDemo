using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace FMBG.AI
{
    /// <summary>状态节点基类：只保存配置，不保存运行时数据。</summary>
    public abstract class EnemyStateNode : Node
    {
        [Input] public EnemyStateNode entry;

        public virtual void Enter(EnemyContext context)
        {
        }

        public virtual void Tick(EnemyContext context, float deltaTime)
        {
        }

        public virtual void Exit(EnemyContext context)
        {
        }

        /// <summary>
        /// 基于可视化条件节点的默认切换评估：
        /// 遍历所有输出端口，找到第一个条件满足的条件节点，返回其 target 连接的状态。
        /// 子类可重写以增加全局守卫（如"技能施法中不切换"）。
        /// </summary>
        public virtual EnemyStateNode EvaluateTransition(EnemyContext context)
        {
            foreach (NodePort port in Outputs)
            {
                if (port == null || port.ConnectionCount == 0)
                {
                    continue;
                }

                for (int i = 0; i < port.ConnectionCount; i++)
                {
                    if (port.GetConnection(i).node is TransitionConditionNode conditionNode &&
                        conditionNode.Evaluate(context))
                    {
                        return conditionNode.GetTargetState();
                    }
                }
            }

            return null;
        }

        protected T GetConnectedNode<T>(string portName) where T : Node
        {
            NodePort port = GetOutputPort(portName);
            if (port == null || port.ConnectionCount == 0)
            {
                return null;
            }

            return port.GetConnection(0).node as T;
        }
    }
}
