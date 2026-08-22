using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace FMBG.AI.Editor
{
    /// <summary>状态节点视图：显示状态名、入口/出口端口与 Chase 参数。</summary>
    public sealed class EnemyStateNodeView : Node
    {
        public EnemyStateNodeData Data { get; }
        public Port InputPort { get; }
        public Port OutputPort { get; }

        public EnemyStateNodeView(EnemyStateNodeData data, Action onChanged)
        {
            Data = data;
            title = EnemyStateNames.Get(data.Type);
            capabilities |= Capabilities.Movable | Capabilities.Selectable | Capabilities.Deletable;

            InputPort = CreatePort(Direction.Input, Port.Capacity.Multi, "In");
            inputContainer.Add(InputPort);

            OutputPort = CreatePort(Direction.Output, Port.Capacity.Multi, "Out");
            outputContainer.Add(OutputPort);

            // Chase 专属参数
            if (data.Type == EnemyStateType.Chase)
            {
                contentContainer.Add(CreateFloatField("重寻路间隔", data.RepathInterval, v =>
                {
                    data.RepathInterval = v;
                    onChanged?.Invoke();
                }));
                contentContainer.Add(CreateFloatField("停止容差", data.StoppingDistanceTolerance, v =>
                {
                    data.StoppingDistanceTolerance = v;
                    onChanged?.Invoke();
                }));
            }

            SetPosition(new Rect(data.Position, Vector2.zero));
            RefreshExpandedState();
        }

        private Port CreatePort(Direction direction, Port.Capacity capacity, string name)
        {
            Port port = InstantiatePort(Orientation.Horizontal, direction, capacity, typeof(EnemyStateNodeView));
            port.portName = name;
            return port;
        }

        internal static FloatField CreateFloatField(string label, float value, Action<float> onChanged)
        {
            var field = new FloatField(label) { value = value };
            field.style.width = 140f;
            field.RegisterValueChangedCallback(evt => onChanged?.Invoke(evt.newValue));
            return field;
        }
    }

    /// <summary>条件节点视图：显示转换条件并允许编辑（类型 + 阈值参数）。</summary>
    public sealed class EnemyConditionNodeView : Node
    {
        public EnemyTransitionData Transition { get; }
        public Port InputPort { get; }
        public Port OutputPort { get; }

        private readonly EnumField typeField;
        private readonly FloatField durationField;
        private readonly FloatField toleranceField;
        private readonly FloatField healthField;

        public EnemyConditionNodeView(EnemyTransitionData transition, Action onChanged)
        {
            Transition = transition;
            title = "条件";
            capabilities |= Capabilities.Movable | Capabilities.Selectable | Capabilities.Deletable;

            InputPort = CreatePort(Direction.Input, Port.Capacity.Single, "From");
            inputContainer.Add(InputPort);

            OutputPort = CreatePort(Direction.Output, Port.Capacity.Single, "To");
            outputContainer.Add(OutputPort);

            typeField = new EnumField("类型", transition.Condition.Type);
            typeField.style.width = 160f;
            typeField.RegisterValueChangedCallback(evt =>
            {
                transition.Condition.SetType((TransitionConditionType)evt.newValue);
                UpdateFields();
                onChanged?.Invoke();
            });
            contentContainer.Add(typeField);

            durationField = EnemyStateNodeView.CreateFloatField("时长(秒)", transition.Condition.Duration, v =>
            {
                transition.Condition.SetDuration(v);
                onChanged?.Invoke();
            });
            toleranceField = EnemyStateNodeView.CreateFloatField("容差", transition.Condition.Tolerance, v =>
            {
                transition.Condition.SetTolerance(v);
                onChanged?.Invoke();
            });
            healthField = EnemyStateNodeView.CreateFloatField("血量阈值", transition.Condition.HealthThreshold, v =>
            {
                transition.Condition.SetHealthThreshold(v);
                onChanged?.Invoke();
            });
            contentContainer.Add(durationField);
            contentContainer.Add(toleranceField);
            contentContainer.Add(healthField);

            UpdateFields();
            SetPosition(new Rect(transition.ConditionPosition, Vector2.zero));
            RefreshExpandedState();
        }

        private void UpdateFields()
        {
            TransitionConditionType t = Transition.Condition.Type;
            durationField.style.display = t == TransitionConditionType.TimerElapsed ? DisplayStyle.Flex : DisplayStyle.None;
            bool range = t == TransitionConditionType.TargetInAttackRange ||
                         t == TransitionConditionType.TargetOutOfAttackRange;
            toleranceField.style.display = range ? DisplayStyle.Flex : DisplayStyle.None;
            healthField.style.display = t == TransitionConditionType.HealthBelow ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private Port CreatePort(Direction direction, Port.Capacity capacity, string name)
        {
            Port port = InstantiatePort(Orientation.Horizontal, direction, capacity, typeof(EnemyStateNodeView));
            port.portName = name;
            return port;
        }
    }

    /// <summary>入口节点视图：不可删除，输出端口连接初始状态。</summary>
    public sealed class EnemyEntryNodeView : Node
    {
        public Port OutputPort { get; }

        public EnemyEntryNodeView()
        {
            title = "Entry";
            capabilities |= Capabilities.Movable | Capabilities.Selectable;

            OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(EnemyStateNodeView));
            OutputPort.portName = "Start";
            outputContainer.Add(OutputPort);
            RefreshExpandedState();
        }
    }

    /// <summary>Any State 节点视图：不可删除，输出端口连接全局死亡目标。</summary>
    public sealed class EnemyAnyStateNodeView : Node
    {
        public Port OutputPort { get; }

        public EnemyAnyStateNodeView()
        {
            title = "Any State";
            capabilities |= Capabilities.Movable | Capabilities.Selectable;

            OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(EnemyStateNodeView));
            OutputPort.portName = "Death";
            outputContainer.Add(OutputPort);
            RefreshExpandedState();
        }
    }
}
