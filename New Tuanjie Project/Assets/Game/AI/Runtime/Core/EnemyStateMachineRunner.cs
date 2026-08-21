using FMBG.Combat;
using UnityEngine;

namespace FMBG.AI
{
    /// <summary>xNode 状态图运行器：全局转换优先 → 当前状态转换 → Tick。</summary>
    public sealed class EnemyStateMachineRunner : MonoBehaviour
    {
        [SerializeField] private EnemyStateGraph graph;
        [SerializeField] private EnemyActor actor;
        [SerializeField] private EnemyPerception perception;
        [SerializeField] private EnemyMotor motor;
        [SerializeField] private CharacterCombat combat;
        [SerializeField] private Health health;

        private EnemyContext context;
        private EnemyStateNode currentState;
        private EnemyAnyStateNode anyStateNode;
        private bool initialized;

        public EnemyStateNode CurrentState => currentState;
        public EnemyContext Context => context;

        public void Initialize(EnemyActor actorRef, EnemyConfig config)
        {
            actor = actorRef;
            graph = config.StateGraph;
            perception = actorRef.Perception;
            motor = actorRef.Motor;
            combat = actorRef.Combat;
            health = actorRef.Health;

            if (graph == null)
            {
                Debug.LogError("EnemyStateMachineRunner: 状态图为空。", this);
                enabled = false;
                return;
            }

            context = new EnemyContext(
                actor,
                config,
                perception,
                motor,
                combat,
                health,
                actorRef.SkillController,
                actorRef.SkillSelector);

            EnemyEntryNode entryNode = graph.FindNode<EnemyEntryNode>();
            anyStateNode = graph.FindNode<EnemyAnyStateNode>();

            if (entryNode == null)
            {
                Debug.LogError("EnemyStateGraph缺少Entry节点。", this);
                enabled = false;
                return;
            }

            initialized = true;
            ChangeState(entryNode.GetStartState());
        }

        private void Update()
        {
            if (!initialized || currentState == null)
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
            if (nextState == null)
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
