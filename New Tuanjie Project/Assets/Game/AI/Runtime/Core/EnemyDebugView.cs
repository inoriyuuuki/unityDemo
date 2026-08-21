using UnityEngine;

namespace FMBG.AI
{
    /// <summary>AI 警戒范围可视化：感知半径 + 视野扇形 + 目标线 + 最后发现位置（Scene 视图始终显示）。</summary>
    public sealed class EnemyDebugView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EnemyActor actor;
        [SerializeField] private EnemyPerception perception;

        [Header("Visuals")]
        public Color viewDistanceColor = new(0f, 0.8f, 1f, 0.35f);
        public Color seeTargetColor = Color.green;
        public Color blockedColor = Color.red;
        public Color lastKnownColor = Color.yellow;
        public Color alertFillColor = new(1f, 0.5f, 0f, 0.08f);

        private void Awake()
        {
            if (actor == null) actor = GetComponent<EnemyActor>();
            if (perception == null) perception = GetComponent<EnemyPerception>();
        }

        private void OnDrawGizmosSelected()
        {
            if (actor == null || actor.Config == null || perception == null)
            {
                return;
            }

            float viewDistance = actor.Config.Perception.viewDistance;
            float viewAngle = actor.Config.Perception.viewAngle;
            Vector3 origin = transform.position + Vector3.up * 0.2f;

            // 警戒范围（感知半径）
            Gizmos.color = viewDistanceColor;
            Gizmos.DrawWireSphere(origin, viewDistance);

            // 视野扇形（网格扇形）
            DrawVisionCone(origin, viewDistance, viewAngle);

            // 目标线
            if (perception.Target != null)
            {
                Vector3 dest = perception.Target.position + Vector3.up * 0.5f;
                Gizmos.color = perception.CanSeeTarget ? seeTargetColor : blockedColor;
                Gizmos.DrawLine(origin, dest);
            }
        }

        private void OnDrawGizmos()
        {
            if (actor == null || actor.Config == null || perception == null)
            {
                return;
            }

            // 最后发现位置
            if (perception.HasLastKnownPosition)
            {
                Gizmos.color = lastKnownColor;
                Gizmos.DrawWireSphere(perception.LastKnownPosition, 0.3f);
            }
        }

        private void DrawVisionCone(Vector3 origin, float radius, float angle)
        {
            const int segments = 24;
            Vector3 forward = transform.forward;

            Vector3 prev = origin + Quaternion.Euler(0f, -angle * 0.5f, 0f) * forward * radius;
            for (int i = 1; i <= segments; i++)
            {
                float a = -angle * 0.5f + (angle * i / segments);
                Vector3 next = origin + Quaternion.Euler(0f, a, 0f) * forward * radius;

                Gizmos.color = viewDistanceColor;
                Gizmos.DrawLine(prev, next);

                // 扇形填充（半透明）
                Gizmos.color = alertFillColor;
                Gizmos.DrawLine(origin, prev);
                Gizmos.DrawLine(origin, next);

                prev = next;
            }
        }
    }
}
