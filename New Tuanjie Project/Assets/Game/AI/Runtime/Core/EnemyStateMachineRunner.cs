using FMBG.Combat;
using UnityEngine;

namespace FMBG.AI
{
    /// <summary>xNode 状态图运行器：全局转换优先 → 当前状态转换 → Tick。</summary>
    public sealed class EnemyStateMachineRunner : MonoBehaviour
    {
        // 所有引用都由 EnemyActor.Initialize 统一注入，不在此序列化，避免出现第二份"真相"。
        private EnemyContext context;
        private EnemyStateNode currentState;
        private EnemyAnyStateNode anyStateNode;
        private bool initialized;

        public EnemyStateNode CurrentState => currentState;
        public EnemyContext Context => context;

        public void Initialize(EnemyActor actor, EnemyConfig config)
        {
            if (actor == null || config == null)
            {
                Debug.LogError("EnemyStateMachineRunner: actor 或 config 为空。", this);
                enabled = false;
                return;
            }

            EnemyStateGraph graph = config.StateGraph;
            if (graph == null)
            {
                Debug.LogError("EnemyStateMachineRunner: 状态图为空。", this);
                enabled = false;
                return;
            }

            context = new EnemyContext(
                actor,
                config,
                actor.Perception,
                actor.Motor,
                actor.Combat,
                actor.Health,
                actor.SkillController,
                actor.SkillSelector);

            EnemyEntryNode entryNode = graph.FindNode<EnemyEntryNode>();
            anyStateNode = graph.FindNode<EnemyAnyStateNode>();

            if (entryNode == null)
            {
                Debug.LogError("EnemyStateGraph缺少Entry节点。", this);
                enabled = false;
                return;
            }

            initialized = true;
            enabled = true; // 确保组件启用（场景可能保存为禁用状态）
            ChangeState(entryNode.GetStartState());
        }

        private void Update()
        {
            if (!initialized || currentState == null || context == null)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            context.Blackboard.StateTime += deltaTime;

            // 全局转换优先（例如死亡）
            EnemyStateNode globalNext = anyStateNode != null
                ? anyStateNode.EvaluateTransition(context)
                : null;

            if (globalNext != null && globalNext != currentState)
            {
                ChangeState(globalNext);
                return;
            }

            // 当前状态转换
            EnemyStateNode nextState = currentState.EvaluateTransition(context);
            if (nextState != null && nextState != currentState)
            {
                ChangeState(nextState);
                return;
            }

            currentState.Tick(context, deltaTime);
        }

        public void ChangeState(EnemyStateNode nextState)
        {
            if (nextState == null || context == null)
            {
                return;
            }

            currentState?.Exit(context);
            currentState = nextState;
            context.Blackboard.StateTime = 0f;
            currentState.Enter(context);
        }

        private void OnDisable()
        {
            if (initialized)
            {
                currentState?.Exit(context);
            }
        }
    }
}
