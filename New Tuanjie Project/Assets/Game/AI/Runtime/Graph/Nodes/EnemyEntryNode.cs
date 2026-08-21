using XNode;

namespace FMBG.AI
{
    [CreateNodeMenu("Enemy AI/Entry")]
    public sealed class EnemyEntryNode : Node
    {
        [Output] public EnemyStateNode start;

        public EnemyStateNode GetStartState()
        {
            NodePort port = GetOutputPort(nameof(start));
            if (port == null || port.ConnectionCount == 0)
            {
                return null;
            }

            return port.GetConnection(0).node as EnemyStateNode;
        }
    }
}
