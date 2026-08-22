using System.Collections.Generic;

namespace FMBG.AI
{
    /// <summary>把 EnemyStateGraph 数据资产编译为运行时状态机对象。</summary>
    public static class EnemyStateMachineCompiler
    {
        public sealed class CompiledGraph
        {
            public EnemyState Entry;
            public EnemyState Death;
        }

        public static CompiledGraph Compile(EnemyStateGraph graph)
        {
            var result = new CompiledGraph();
            if (graph == null)
            {
                return result;
            }

            var map = new Dictionary<string, EnemyState>();
            for (int i = 0; i < graph.States.Count; i++)
            {
                EnemyStateNodeData data = graph.States[i];
                if (data == null || string.IsNullOrEmpty(data.Id))
                {
                    continue;
                }
                EnemyState state = CreateState(data);
                if (state != null)
                {
                    map[data.Id] = state;
                }
            }

            for (int i = 0; i < graph.Transitions.Count; i++)
            {
                EnemyTransitionData t = graph.Transitions[i];
                if (t == null)
                {
                    continue;
                }
                if (!map.TryGetValue(t.FromStateId, out EnemyState from) ||
                    !map.TryGetValue(t.ToStateId, out EnemyState to) ||
                    t.Condition == null)
                {
                    continue;
                }
                from.Transitions.Add(new EnemyTransition { Condition = t.Condition, Target = to });
            }

            if (!string.IsNullOrEmpty(graph.EntryStateId))
            {
                map.TryGetValue(graph.EntryStateId, out result.Entry);
            }
            if (!string.IsNullOrEmpty(graph.AnyStateDeathTargetId))
            {
                map.TryGetValue(graph.AnyStateDeathTargetId, out result.Death);
            }

            return result;
        }

        private static EnemyState CreateState(EnemyStateNodeData data)
        {
            switch (data.Type)
            {
                case EnemyStateType.Idle: return new IdleState(data);
                case EnemyStateType.Patrol: return new PatrolState(data);
                case EnemyStateType.Chase: return new ChaseState(data);
                case EnemyStateType.Attack: return new AttackState(data);
                case EnemyStateType.Investigate: return new InvestigateState(data);
                case EnemyStateType.Return: return new ReturnState(data);
                case EnemyStateType.Dead: return new DeadState(data);
                default: return null;
            }
        }
    }
}
