using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace FMBG.AI.Editor
{
    /// <summary>
    /// GraphView 状态机画布：编辑 <see cref="EnemyStateGraph"/> 数据资产。
    /// 交互规则：状态 → 状态 拖线自动在中间插入条件节点；删除条件节点/连线 = 删除整条转换。
    /// </summary>
    public sealed class EnemyStateMachineGraphView : GraphView
    {
        private EnemyStateGraph graphAsset;
        private readonly Dictionary<string, EnemyStateNodeView> stateViews = new();
        private readonly Dictionary<string, EnemyConditionNodeView> conditionViews = new();
        private readonly Dictionary<string, Edge> stateToConditionEdges = new();
        private readonly Dictionary<string, Edge> conditionToStateEdges = new();
        private EnemyEntryNodeView entryView;
        private EnemyAnyStateNodeView anyStateView;
        private Edge entryEdge;
        private Edge deathEdge;

        public EnemyStateGraph GraphAsset => graphAsset;

        public EnemyStateMachineGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new FreehandSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            graphViewChanged = OnGraphViewChanged;
        }

        public void LoadGraph(EnemyStateGraph graph)
        {
            ClearGraph();
            graphAsset = graph;
            if (graphAsset == null)
            {
                return;
            }
            BuildGraph();
        }

        public void Save()
        {
            if (graphAsset == null)
            {
                return;
            }
            EditorUtility.SetDirty(graphAsset);
            AssetDatabase.SaveAssets();
        }

        private void ClearGraph()
        {
            foreach (var edge in stateToConditionEdges.Values)
            {
                if (Contains(edge)) RemoveElement(edge);
            }
            foreach (var edge in conditionToStateEdges.Values)
            {
                if (Contains(edge)) RemoveElement(edge);
            }
            if (entryEdge != null && Contains(entryEdge)) RemoveElement(entryEdge);
            if (deathEdge != null && Contains(deathEdge)) RemoveElement(deathEdge);

            foreach (var view in conditionViews.Values)
            {
                if (Contains(view)) RemoveElement(view);
            }
            foreach (var view in stateViews.Values)
            {
                if (Contains(view)) RemoveElement(view);
            }
            if (entryView != null && Contains(entryView)) RemoveElement(entryView);
            if (anyStateView != null && Contains(anyStateView)) RemoveElement(anyStateView);

            stateViews.Clear();
            conditionViews.Clear();
            stateToConditionEdges.Clear();
            conditionToStateEdges.Clear();
            entryView = null;
            anyStateView = null;
            entryEdge = null;
            deathEdge = null;
        }

        private void BuildGraph()
        {
            entryView = new EnemyEntryNodeView();
            entryView.SetPosition(new Rect(graphAsset.EntryNodePosition, Vector2.zero));
            AddElement(entryView);

            anyStateView = new EnemyAnyStateNodeView();
            anyStateView.SetPosition(new Rect(graphAsset.AnyStateNodePosition, Vector2.zero));
            AddElement(anyStateView);

            for (int i = 0; i < graphAsset.States.Count; i++)
            {
                EnemyStateNodeData data = graphAsset.States[i];
                if (data == null || string.IsNullOrEmpty(data.Id))
                {
                    continue;
                }
                var view = new EnemyStateNodeView(data, OnDataChanged);
                AddElement(view);
                stateViews[data.Id] = view;
            }

            for (int i = 0; i < graphAsset.Transitions.Count; i++)
            {
                EnemyTransitionData t = graphAsset.Transitions[i];
                if (t == null || string.IsNullOrEmpty(t.Id))
                {
                    continue;
                }
                var condView = new EnemyConditionNodeView(t, OnDataChanged);
                AddElement(condView);
                conditionViews[t.Id] = condView;

                if (stateViews.TryGetValue(t.FromStateId, out EnemyStateNodeView fromView))
                {
                    stateToConditionEdges[t.Id] = Connect(fromView.OutputPort, condView.InputPort);
                }
                if (stateViews.TryGetValue(t.ToStateId, out EnemyStateNodeView toView))
                {
                    conditionToStateEdges[t.Id] = Connect(condView.OutputPort, toView.InputPort);
                }
            }

            if (!string.IsNullOrEmpty(graphAsset.EntryStateId) &&
                stateViews.TryGetValue(graphAsset.EntryStateId, out EnemyStateNodeView entryState))
            {
                entryEdge = Connect(entryView.OutputPort, entryState.InputPort);
            }

            if (!string.IsNullOrEmpty(graphAsset.AnyStateDeathTargetId) &&
                stateViews.TryGetValue(graphAsset.AnyStateDeathTargetId, out EnemyStateNodeView deathState))
            {
                deathEdge = Connect(anyStateView.OutputPort, deathState.InputPort);
            }
        }

        private Edge Connect(Port from, Port to)
        {
            var edge = new Edge { output = from, input = to };
            from.Connect(edge);
            to.Connect(edge);
            AddElement(edge);
            return edge;
        }

        private Edge MakeEdge(Port from, Port to)
        {
            var edge = new Edge { output = from, input = to };
            from.Connect(edge);
            to.Connect(edge);
            return edge;
        }

        // ---------- graphViewChanged：创建 / 删除 / 移动 ----------

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (change.edgesToCreate != null && change.edgesToCreate.Count > 0)
            {
                var created = new List<Edge>();
                for (int i = 0; i < change.edgesToCreate.Count; i++)
                {
                    created.AddRange(CreateEdgeData(change.edgesToCreate[i]));
                }
                change.edgesToCreate = created;
            }

            if (change.elementsToRemove != null && change.elementsToRemove.Count > 0)
            {
                for (int i = 0; i < change.elementsToRemove.Count; i++)
                {
                    RemoveElementData(change.elementsToRemove[i]);
                }
            }

            if (change.movedElements != null && change.movedElements.Count > 0)
            {
                for (int i = 0; i < change.movedElements.Count; i++)
                {
                    MoveElementData(change.movedElements[i]);
                }
            }

            Save();
            return change;
        }

        private List<Edge> CreateEdgeData(Edge edge)
        {
            Port fromPort = edge.output;
            Port toPort = edge.input;
            if (fromPort == null || toPort == null)
            {
                return new List<Edge>();
            }

            Node fromNode = fromPort.node;
            Node toNode = toPort.node;

            // 状态 → 状态：自动插入条件节点
            if (fromNode is EnemyStateNodeView fromState && toNode is EnemyStateNodeView toState)
            {
                if (fromState == toState)
                {
                    return new List<Edge>();
                }
                return CreateConditionBetween(fromState, toState);
            }

            // Entry → 状态：设置初始状态
            if (fromNode is EnemyEntryNodeView && toNode is EnemyStateNodeView entryTarget)
            {
                graphAsset.SetEntry(entryTarget.Data.Id);
                return new List<Edge> { edge };
            }

            // Any State → 状态：设置全局死亡目标
            if (fromNode is EnemyAnyStateNodeView && toNode is EnemyStateNodeView deathTarget)
            {
                graphAsset.SetDeathTarget(deathTarget.Data.Id);
                return new List<Edge> { edge };
            }

            // 状态 → 条件：设置转换起点
            if (fromNode is EnemyStateNodeView condFrom && toNode is EnemyConditionNodeView condIn)
            {
                condIn.Transition.FromStateId = condFrom.Data.Id;
                return new List<Edge> { edge };
            }

            // 条件 → 状态：设置转换终点
            if (fromNode is EnemyConditionNodeView condOut && toNode is EnemyStateNodeView stateTarget)
            {
                condOut.Transition.ToStateId = stateTarget.Data.Id;
                return new List<Edge> { edge };
            }

            // 非法连接：丢弃
            return new List<Edge>();
        }

        private List<Edge> CreateConditionBetween(EnemyStateNodeView fromState, EnemyStateNodeView toState)
        {
            Vector2 mid = (fromState.GetPosition().position + toState.GetPosition().position) * 0.5f;

            EnemyTransitionData transition = graphAsset.AddTransition(
                fromState.Data.Id,
                toState.Data.Id,
                new TransitionCondition(TransitionConditionType.Always),
                mid);

            var condView = new EnemyConditionNodeView(transition, OnDataChanged);
            AddElement(condView);
            conditionViews[transition.Id] = condView;

            Edge e1 = MakeEdge(fromState.OutputPort, condView.InputPort);
            Edge e2 = MakeEdge(condView.OutputPort, toState.InputPort);
            stateToConditionEdges[transition.Id] = e1;
            conditionToStateEdges[transition.Id] = e2;
            return new List<Edge> { e1, e2 };
        }

        private void RemoveElementData(GraphElement element)
        {
            if (element is EnemyStateNodeView stateView)
            {
                var transitionIds = new List<string>();
                for (int i = 0; i < graphAsset.Transitions.Count; i++)
                {
                    EnemyTransitionData t = graphAsset.Transitions[i];
                    if (t != null && (t.FromStateId == stateView.Data.Id || t.ToStateId == stateView.Data.Id))
                    {
                        transitionIds.Add(t.Id);
                    }
                }
                for (int i = 0; i < transitionIds.Count; i++)
                {
                    RemoveTransitionView(transitionIds[i]);
                }

                graphAsset.RemoveState(stateView.Data.Id);
                stateViews.Remove(stateView.Data.Id);
                if (entryEdge != null && entryEdge.input?.node == stateView)
                {
                    entryEdge = null;
                }
                if (deathEdge != null && deathEdge.input?.node == stateView)
                {
                    deathEdge = null;
                }
                return;
            }

            if (element is EnemyConditionNodeView condView)
            {
                RemoveTransitionView(condView.Transition.Id);
                return;
            }

            if (element is Edge edge)
            {
                if (edge == entryEdge)
                {
                    graphAsset.SetEntry(null);
                    entryEdge = null;
                    return;
                }
                if (edge == deathEdge)
                {
                    graphAsset.SetDeathTarget(null);
                    deathEdge = null;
                    return;
                }

                string transitionId = FindTransitionIdByEdge(edge);
                if (transitionId != null)
                {
                    RemoveTransitionView(transitionId);
                }
            }
        }

        private void RemoveTransitionView(string transitionId)
        {
            if (string.IsNullOrEmpty(transitionId))
            {
                return;
            }

            if (conditionViews.TryGetValue(transitionId, out EnemyConditionNodeView condView))
            {
                if (stateToConditionEdges.TryGetValue(transitionId, out Edge e1))
                {
                    if (Contains(e1)) RemoveElement(e1);
                    stateToConditionEdges.Remove(transitionId);
                }
                if (conditionToStateEdges.TryGetValue(transitionId, out Edge e2))
                {
                    if (Contains(e2)) RemoveElement(e2);
                    conditionToStateEdges.Remove(transitionId);
                }
                if (Contains(condView)) RemoveElement(condView);
                conditionViews.Remove(transitionId);
            }

            graphAsset.RemoveTransition(transitionId);
        }

        private string FindTransitionIdByEdge(Edge edge)
        {
            foreach (var kv in stateToConditionEdges)
            {
                if (kv.Value == edge) return kv.Key;
            }
            foreach (var kv in conditionToStateEdges)
            {
                if (kv.Value == edge) return kv.Key;
            }
            return null;
        }

        private void MoveElementData(GraphElement element)
        {
            if (graphAsset == null)
            {
                return;
            }
            if (element is EnemyStateNodeView stateView)
            {
                stateView.Data.Position = stateView.GetPosition().position;
            }
            else if (element is EnemyConditionNodeView condView)
            {
                condView.Transition.ConditionPosition = condView.GetPosition().position;
            }
            else if (element is EnemyEntryNodeView && entryView != null)
            {
                graphAsset.EntryNodePosition = entryView.GetPosition().position;
            }
            else if (element is EnemyAnyStateNodeView && anyStateView != null)
            {
                graphAsset.AnyStateNodePosition = anyStateView.GetPosition().position;
            }
        }

        private void OnDataChanged()
        {
            Save();
        }

        // ---------- 右键菜单：添加状态 ----------

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            if (graphAsset == null)
            {
                return;
            }

            Vector2 localPos = contentViewContainer.WorldToLocal(evt.mousePosition);
            foreach (EnemyStateType type in System.Enum.GetValues(typeof(EnemyStateType)))
            {
                EnemyStateType captured = type;
                evt.menu.AppendAction(
                    $"添加状态/{EnemyStateNames.Get(type)}",
                    _ => AddStateAt(captured, localPos));
            }
        }

        private void AddStateAt(EnemyStateType type, Vector2 position)
        {
            if (graphAsset == null)
            {
                return;
            }
            EnemyStateNodeData data = graphAsset.AddState(type, position);
            var view = new EnemyStateNodeView(data, OnDataChanged);
            AddElement(view);
            stateViews[data.Id] = view;
            Save();
        }
    }
}
