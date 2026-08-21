using FMBG.Combat;
using UnityEngine;

namespace FMBG.AI
{
    /// <summary>AI 调试可视化：头顶状态文字 + 感知半径/视野扇形 Gizmos + 目标线 + F1 开关。</summary>
    public sealed class EnemyDebugView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EnemyActor actor;
        [SerializeField] private EnemyPerception perception;
        [SerializeField] private EnemyMotor motor;

        [Header("Visuals")]
        public Color viewDistanceColor = new(0f, 0.8f, 1f, 0.3f);
        public Color seeTargetColor = Color.green;
        public Color blockedColor = Color.red;
        public Color lastKnownColor = Color.yellow;

        public static bool DebugEnabled { get; set; } = true;

        private GUIStyle labelStyle;
        private bool initialized;

        private void Awake()
        {
            if (actor == null) actor = GetComponent<EnemyActor>();
            if (perception == null) perception = GetComponent<EnemyPerception>();
            if (motor == null) motor = GetComponent<EnemyMotor>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                DebugEnabled = !DebugEnabled;
            }
        }

        private void OnGUI()
        {
            if (!DebugEnabled || actor == null)
            {
                return;
            }

            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold
                };
            }

            var state = actor.StateMachine != null && actor.StateMachine.CurrentState != null
                ? actor.StateMachine.CurrentState.GetType().Name.Replace("StateNode", "")
                : "None";

            Vector3 screenPos = Camera.main != null
                ? Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2.5f)
                : Vector3.zero;

            if (screenPos.z > 0)
            {
                string text = $"[{state}]";
                Vector2 size = labelStyle.CalcSize(new GUIContent(text));
                GUI.Label(
                    new Rect(screenPos.x - size.x * 0.5f, Screen.height - screenPos.y, size.x, size.y),
                    text,
                    labelStyle);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!DebugEnabled || perception == null)
            {
                return;
            }

            // 感知半径
            Gizmos.color = viewDistanceColor;
            Gizmos.DrawWireSphere(transform.position, GetViewDistance());

            // 视野扇形（简化为两条边线）
            float viewDistance = GetViewDistance();
            float viewAngle = GetViewAngle();
            Vector3 forward = transform.forward;
            Vector3 left = Quaternion.Euler(0f, -viewAngle * 0.5f, 0f) * forward;
            Vector3 right = Quaternion.Euler(0f, viewAngle * 0.5f, 0f) * forward;

            Gizmos.color = seeTargetColor;
            Gizmos.DrawRay(transform.position, left * viewDistance);
            Gizmos.DrawRay(transform.position, right * viewDistance);

            // 目标线
            if (perception.Target != null)
            {
                Vector3 origin = transform.position + Vector3.up * 0.5f;
                Vector3 dest = perception.Target.position + Vector3.up * 0.5f;
                Gizmos.color = perception.CanSeeTarget ? seeTargetColor : blockedColor;
                Gizmos.DrawLine(origin, dest);
            }
        }

        private void OnDrawGizmos()
        {
            if (!DebugEnabled || actor == null || actor.Config == null)
            {
                return;
            }

            // 最后发现位置
            if (perception != null && perception.HasLastKnownPosition)
            {
                Gizmos.color = lastKnownColor;
                Gizmos.DrawWireSphere(perception.LastKnownPosition, 0.3f);
            }
        }

        private float GetViewDistance()
        {
            return actor != null && actor.Config != null ? actor.Config.Perception.viewDistance : 10f;
        }

        private float GetViewAngle()
        {
            return actor != null && actor.Config != null ? actor.Config.Perception.viewAngle : 100f;
        }
    }
}
