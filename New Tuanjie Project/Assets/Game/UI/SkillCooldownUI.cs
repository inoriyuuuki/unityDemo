using FMBG.Characters;
using FMBG.Skills;
using UnityEngine;

namespace FMBG.UI
{
    /// <summary>技能冷却 UI：显示近战/远程技能的冷却状态与剩余时间。</summary>
    public sealed class SkillCooldownUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterSkillController skillController;
        [SerializeField] private PlayerCombatInput playerInput;

        [Header("Style")]
        [SerializeField] private float boxSize = 64f;
        [SerializeField] private float spacing = 12f;
        [SerializeField] private Color readyColor = new(0.9f, 0.9f, 0.9f, 0.7f);
        [SerializeField] private Color cooldownColor = new(0.3f, 0.3f, 0.3f, 0.8f);
        [SerializeField] private Color cdTextColor = Color.white;

        private GUIStyle labelStyle;

        private void OnGUI()
        {
            if (skillController == null || playerInput == null)
            {
                return;
            }

            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 18,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
            }

            float x = Screen.width - boxSize - 20f;
            float y = Screen.height - boxSize - 20f;

            DrawSkillBox(new Rect(x, y, boxSize, boxSize), "近战", playerInput.MeleeSkill);
            DrawSkillBox(new Rect(x - boxSize - spacing, y, boxSize, boxSize), "远程", playerInput.RangedSkill);
        }

        private void DrawSkillBox(Rect rect, string label, SkillConfig skill)
        {
            bool onCooldown = skill != null && skillController.IsOnCooldown(skill);
            GUI.color = onCooldown ? cooldownColor : readyColor;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            // 名称
            var nameStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.UpperCenter
            };
            GUI.Label(rect, label, nameStyle);

            // 冷却剩余时间
            if (onCooldown && skill != null)
            {
                float remaining = skillController.GetCooldownRemaining(skill);
                GUI.color = cdTextColor;
                GUI.Label(rect, remaining.ToString("0.0"), labelStyle);
                GUI.color = Color.white;
            }
            else
            {
                GUI.Label(rect, "就绪", labelStyle);
            }
        }
    }
}
