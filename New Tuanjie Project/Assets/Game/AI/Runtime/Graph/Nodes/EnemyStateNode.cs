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

        public virtual EnemyStateNode EvaluateTransition(EnemyContext context)
        {
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
