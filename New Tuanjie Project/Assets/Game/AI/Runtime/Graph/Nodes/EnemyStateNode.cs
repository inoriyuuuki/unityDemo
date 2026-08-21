using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace FMBG.AI
{
    /// <summary>状态节点基类：只保存配置，不保存运行时数据。</summary>
    public abstract class EnemyStateNode : Node
    {
        [Input] public EnemyStateNode entry;

        [Header("Transition Conditions")]
        [Tooltip("输出端口对应的切换条件。按顺序评估，第一个满足的条件触发切换。")]
        [SerializeField] private List<PortCondition> portConditions = new();

        /// <summary>配置的端口条件列表（Inspector 可编辑）。</summary>
        public List<PortCondition> PortConditions => portConditions;

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
        /// 基于端口条件的默认切换评估：
        /// 遍历配置的 PortCondition，第一个满足的返回其端口连接的节点。
        /// 子类可重写以增加全局守卫（如"技能施法中不切换"）。
        /// </summary>
        public virtual EnemyStateNode EvaluateTransition(EnemyContext context)
        {
            if (portConditions == null)
            {
                return null;
            }

            foreach (var pc in portConditions)
            {
                if (pc == null || string.IsNullOrEmpty(pc.PortName) || pc.Condition == null)
                {
                    continue;
                }

                if (pc.Condition.Evaluate(context))
                {
                    return GetConnectedNode<EnemyStateNode>(pc.PortName);
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

    /// <summary>端口与条件的绑定（可序列化）。</summary>
    [System.Serializable]
    public sealed class PortCondition
    {
        [SerializeField] private string portName;
        [SerializeField] private TransitionCondition condition = new();

        public string PortName => portName;
        public TransitionCondition Condition => condition;

        public PortCondition() { }

        public PortCondition(string portName, TransitionConditionType type)
        {
            this.portName = portName;
            condition = new TransitionCondition(type);
        }
    }
}
