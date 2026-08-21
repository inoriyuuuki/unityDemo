using System.Linq;
using UnityEngine;
using XNode;

namespace FMBG.AI
{
    [CreateAssetMenu(
        fileName = "EnemyStateGraph",
        menuName = "Game/AI/Enemy State Graph")]
    public sealed class EnemyStateGraph : NodeGraph
    {
        public T FindNode<T>() where T : Node
        {
            return nodes.OfType<T>().FirstOrDefault();
        }
    }
}
