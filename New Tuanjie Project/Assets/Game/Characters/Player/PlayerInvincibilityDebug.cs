using FMBG.Combat;
using UnityEngine;

namespace FMBG.Characters
{
    /// <summary>F1 切换玩家无敌模式，并在左上角通过 IMGUI 显示状态。</summary>
    [RequireComponent(typeof(Health))]
    public sealed class PlayerInvincibilityDebug : MonoBehaviour
    {
        [SerializeField] private KeyCode toggleKey = KeyCode.F1;
        [SerializeField] private bool showDisabledHint = true;

        private Health health;
        private GUIStyle labelStyle;

        private void Awake()
        {
            health = GetComponent<Health>();
        }

        private void Update()
        {
            if (health != null && Input.GetKeyDown(toggleKey))
            {
                health.SetInvincible(!health.IsInvincible);
            }
        }

        private void OnGUI()
        {
            if (health == null || (!showDisabledHint && !health.IsInvincible))
            {
                return;
            }

            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = 18,
                    fontStyle = FontStyle.Bold,
                    padding = new RectOffset(12, 12, 6, 6)
                };
            }

            bool enabled = health.IsInvincible;
            labelStyle.normal.textColor = enabled
                ? new Color(0.45f, 1f, 0.45f)
                : Color.white;

            Rect rect = new(12f, 12f, 310f, 38f);
            Color previousColor = GUI.color;
            GUI.color = enabled
                ? new Color(0.05f, 0.35f, 0.08f, 0.9f)
                : new Color(0f, 0f, 0f, 0.65f);
            GUI.Box(rect, GUIContent.none);
            GUI.color = Color.white;
            GUI.Label(rect, enabled
                ? "[F1] 玩家无敌模式：已开启"
                : "[F1] 玩家无敌模式：已关闭", labelStyle);
            GUI.color = previousColor;
        }
    }
}
