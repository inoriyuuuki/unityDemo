using XNode;

namespace FMBG.AI
{
    [CreateNodeMenu("Enemy AI/Any State")]
    public sealed class EnemyAnyStateNode : Node
    {
        [Output] public EnemyStateNode dead;

        public EnemyStateNode EvaluateTransition(EnemyContext context)
        {
            if (context.Health != null && !context.Health.IsAlive)
            {
                return GetConnectedState(nameof(dead));
            }

            return null;
        }

        private EnemyStateNode GetConnectedState(string portName)
        {
            NodePort port = GetOutputPort(portName);
            if (port == null || port.ConnectionCount == 0)
            {
                return null;
            }

            return port.GetConnection(0).node as EnemyStateNode;
        }
    }
}
