using FMBG.Combat;
using UnityEngine;

namespace FMBG.AI
{
    /// <summary>状态机运行器：全局死亡转换优先 → 当前状态转换 → Tick。</summary>
    public sealed class EnemyStateMachineRunner : MonoBehaviour
    {
        // 所有引用都由 EnemyActor.Initialize 统一注入，不在此序列化，避免出现第二份"真相"。
        private EnemyContext context;
        private EnemyState currentState;
        private EnemyState deathState;
        private bool initialized;

        public EnemyState CurrentState => currentState;
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

            EnemyStateMachineCompiler.CompiledGraph compiled = EnemyStateMachineCompiler.Compile(graph);
            if (compiled.Entry == null)
            {
                Debug.LogError("EnemyStateGraph 缺少有效 Entry 状态。", this);
                enabled = false;
                return;
            }

            deathState = compiled.Death;
            initialized = true;
            enabled = true; // 确保组件启用（场景可能保存为禁用状态）
            ChangeState(compiled.Entry);
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
            if (deathState != null &&
                deathState != currentState &&
                context.Health != null &&
                !context.Health.IsAlive)
            {
                ChangeState(deathState);
                return;
            }

            // 当前状态转换
            EnemyState nextState = currentState.EvaluateTransitions(context);
            if (nextState != null && nextState != currentState)
            {
                ChangeState(nextState);
                return;
            }

            currentState.Tick(context, deltaTime);
        }

        public void ChangeState(EnemyState nextState)
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
