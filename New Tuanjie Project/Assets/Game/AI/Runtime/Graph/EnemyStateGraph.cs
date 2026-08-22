using System.Collections.Generic;
using UnityEngine;

namespace FMBG.AI
{
    /// <summary>
    /// 敌人状态机图（数据资产）：纯序列化数据模型，不依赖任何编辑器框架。
    /// 由 GraphView 编辑器可视化编辑；运行时由 <see cref="EnemyStateMachineCompiler"/> 编译执行。
    /// </summary>
    [CreateAssetMenu(
        fileName = "EnemyStateGraph",
        menuName = "Game/AI/Enemy State Graph")]
    public sealed class EnemyStateGraph : ScriptableObject
    {
        [SerializeField] private string entryStateId;
        [SerializeField] private string anyStateDeathTargetId;
        [SerializeField] private Vector2 entryNodePosition = new(-600f, 0f);
        [SerializeField] private Vector2 anyStateNodePosition = new(300f, 300f);
        [SerializeField] private List<EnemyStateNodeData> states = new();
        [SerializeField] private List<EnemyTransitionData> transitions = new();

        public string EntryStateId => entryStateId;
        public string AnyStateDeathTargetId => anyStateDeathTargetId;
        public Vector2 EntryNodePosition { get => entryNodePosition; set => entryNodePosition = value; }
        public Vector2 AnyStateNodePosition { get => anyStateNodePosition; set => anyStateNodePosition = value; }
        public IReadOnlyList<EnemyStateNodeData> States => states;
        public IReadOnlyList<EnemyTransitionData> Transitions => transitions;

        public void Clear()
        {
            states.Clear();
            transitions.Clear();
            entryStateId = null;
            anyStateDeathTargetId = null;
        }

        public EnemyStateNodeData FindState(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }
            for (int i = 0; i < states.Count; i++)
            {
                if (states[i] != null && states[i].Id == id)
                {
                    return states[i];
                }
            }
            return null;
        }

        public EnemyTransitionData FindTransition(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }
            for (int i = 0; i < transitions.Count; i++)
            {
                if (transitions[i] != null && transitions[i].Id == id)
                {
                    return transitions[i];
                }
            }
            return null;
        }

        public EnemyStateNodeData AddState(EnemyStateType type, Vector2 position)
        {
            var data = new EnemyStateNodeData(System.Guid.NewGuid().ToString("N"), type, position);
            states.Add(data);
            return data;
        }

        /// <summary>移除状态及其所有相关转换。</summary>
        public bool RemoveState(string id)
        {
            for (int i = transitions.Count - 1; i >= 0; i--)
            {
                if (transitions[i] != null &&
                    (transitions[i].FromStateId == id || transitions[i].ToStateId == id))
                {
                    transitions.RemoveAt(i);
                }
            }
            if (entryStateId == id)
            {
                entryStateId = null;
            }
            if (anyStateDeathTargetId == id)
            {
                anyStateDeathTargetId = null;
            }
            for (int i = states.Count - 1; i >= 0; i--)
            {
                if (states[i] != null && states[i].Id == id)
                {
                    states.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public EnemyTransitionData AddTransition(
            string fromStateId,
            string toStateId,
            TransitionCondition condition,
            Vector2 conditionPosition)
        {
            var data = new EnemyTransitionData(
                System.Guid.NewGuid().ToString("N"),
                fromStateId,
                toStateId,
                condition,
                conditionPosition);
            transitions.Add(data);
            return data;
        }

        public bool RemoveTransition(string id)
        {
            for (int i = transitions.Count - 1; i >= 0; i--)
            {
                if (transitions[i] != null && transitions[i].Id == id)
                {
                    transitions.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public void SetEntry(string stateId) => entryStateId = stateId;
        public void SetDeathTarget(string stateId) => anyStateDeathTargetId = stateId;

        /// <summary>校验图配置，返回错误列表（空列表 = 通过）。</summary>
        public List<string> GetValidationErrors()
        {
            var errors = new List<string>();
            if (states.Count == 0)
            {
                errors.Add("图中没有状态节点。");
                return errors;
            }

            if (string.IsNullOrEmpty(entryStateId) || FindState(entryStateId) == null)
            {
                errors.Add("缺少 Entry 或 Entry 未连接有效初始状态。");
            }

            if (string.IsNullOrEmpty(anyStateDeathTargetId) || FindState(anyStateDeathTargetId) == null)
            {
                errors.Add("缺少全局死亡目标（Any State 未连接）。");
            }

            for (int i = 0; i < states.Count; i++)
            {
                EnemyStateNodeData state = states[i];
                if (state == null || state.Type == EnemyStateType.Dead)
                {
                    continue;
                }
                bool hasOut = false;
                for (int j = 0; j < transitions.Count; j++)
                {
                    if (transitions[j] != null && transitions[j].FromStateId == state.Id)
                    {
                        hasOut = true;
                        break;
                    }
                }
                if (!hasOut)
                {
                    errors.Add($"{EnemyStateNames.Get(state.Type)} 没有出向转换。");
                }
            }

            for (int i = 0; i < transitions.Count; i++)
            {
                EnemyTransitionData t = transitions[i];
                if (t == null)
                {
                    continue;
                }
                if (FindState(t.FromStateId) == null)
                {
                    errors.Add("存在无效的转换起点（from 状态不存在）。");
                }
                if (FindState(t.ToStateId) == null)
                {
                    errors.Add("存在无效的转换终点（to 状态不存在）。");
                }
                if (t.Condition == null)
                {
                    errors.Add("存在未配置条件的转换。");
                }
            }

            return errors;
        }
    }
}
