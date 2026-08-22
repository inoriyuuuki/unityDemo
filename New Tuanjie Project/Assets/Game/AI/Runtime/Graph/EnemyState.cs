using System.Collections.Generic;

namespace FMBG.AI
{
    /// <summary>转换实例：条件 + 目标状态（运行时编译产物）。</summary>
    public sealed class EnemyTransition
    {
        public TransitionCondition Condition;
        public EnemyState Target;

        public bool Evaluate(EnemyContext context)
        {
            return Condition != null && Condition.Evaluate(context);
        }
    }

    /// <summary>运行时状态基类：只持有配置与转换，不持有组件引用。</summary>
    public abstract class EnemyState
    {
        public string Id { get; }
        public EnemyStateType Type { get; }
        public List<EnemyTransition> Transitions { get; } = new();

        protected EnemyState(EnemyStateNodeData data)
        {
            Id = data != null ? data.Id : string.Empty;
            Type = data != null ? data.Type : EnemyStateType.Idle;
        }

        public virtual void Enter(EnemyContext context)
        {
        }

        public virtual void Tick(EnemyContext context, float deltaTime)
        {
        }

        public virtual void Exit(EnemyContext context)
        {
        }

        /// <summary>按顺序评估出向转换，返回第一个满足条件的目标状态。</summary>
        public virtual EnemyState EvaluateTransitions(EnemyContext context)
        {
            for (int i = 0; i < Transitions.Count; i++)
            {
                EnemyTransition t = Transitions[i];
                if (t != null && t.Target != null && t.Evaluate(context))
                {
                    return t.Target;
                }
            }
            return null;
        }
    }
}
