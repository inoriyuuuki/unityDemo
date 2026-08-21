using System.Collections.Generic;
using FMBG.AI;
using FMBG.Combat;
using UnityEngine;

namespace FMBG.UI
{
    /// <summary>HUD：在角色头顶显示血条（世界跟随），敌人额外显示警戒值。</summary>
    public sealed class HealthBarHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Health playerHealth;
        [SerializeField] private Health[] enemyHealths;

        [Header("Style")]
        [SerializeField] private float barWidth = 1.6f;      // 世界单位宽度
        [SerializeField] private float barHeightWorld = 0.18f;
        [SerializeField] private float heightAboveHead = 2.4f;
        [SerializeField] private Color playerBarColor = new(0.2f, 0.8f, 0.3f);
        [SerializeField] private Color enemyBarColor = new(0.9f, 0.3f, 0.3f);
        [SerializeField] private Color bgColor = new(0f, 0f, 0f, 0.65f);
        [SerializeField] private Color alertColor = new(1f, 0.85f, 0.2f);
        [SerializeField] private Color stateColor = new(1f, 1f, 1f);

        private GUIStyle barLabelStyle;
        private GUIStyle alertLabelStyle;
        private GUIStyle stateLabelStyle;

        public void SetTargets(Health player, IEnumerable<Health> enemies)
        {
            playerHealth = player;
            enemyHealths = enemies == null ? null : new List<Health>(enemies).ToArray();
        }

        private void OnGUI()
        {
            if (barLabelStyle == null)
            {
                barLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                alertLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    alignment = TextAnchor.MiddleCenter
                };
                stateLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
            }

            if (Camera.main == null)
            {
                return;
            }

            if (playerHealth != null && playerHealth.gameObject.activeInHierarchy)
            {
                DrawWorldBar(playerHealth.transform, playerHealth, playerBarColor, null, null);
            }

            if (enemyHealths != null)
            {
                for (int i = 0; i < enemyHealths.Length; i++)
                {
                    var enemy = enemyHealths[i];
                    if (enemy == null || !enemy.gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    var perception = enemy.GetComponentInParent<EnemyPerception>();
                    var actor = enemy.GetComponentInParent<EnemyActor>();
                    string state = actor != null && actor.StateMachine != null && actor.StateMachine.CurrentState != null
                        ? actor.StateMachine.CurrentState.GetType().Name.Replace("StateNode", "")
                        : null;
                    DrawWorldBar(enemy.transform, enemy, enemyBarColor, perception, state);
                }
            }
        }

        private void DrawWorldBar(Transform target, Health health, Color barColor, EnemyPerception perception, string state)
        {
            Vector3 worldPos = target.position + Vector3.up * heightAboveHead;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            // 在相机背后不绘制
            if (screenPos.z <= 0)
            {
                return;
            }

            // 根据距离缩放（世界单位 → 像素）
            float distance = Vector3.Distance(Camera.main.transform.position, target.position);
            float scale = Mathf.Clamp(60f / Mathf.Max(1f, distance), 0.5f, 2f);

            float pixelWidth = barWidth * 100f * scale;
            float pixelHeight = barHeightWorld * 100f * scale;

            float x = screenPos.x - pixelWidth * 0.5f;
            float y = Screen.height - screenPos.y - pixelHeight * 0.5f;

            var bgRect = new Rect(x, y, pixelWidth, pixelHeight);

            // 背景
            GUI.color = bgColor;
            GUI.DrawTexture(bgRect, Texture2D.whiteTexture);

            // 血条填充
            float ratio = health != null ? Mathf.Clamp01(health.CurrentHealth / health.MaxHealth) : 0f;
            if (ratio > 0f)
            {
                GUI.color = barColor;
                GUI.DrawTexture(new Rect(x, y, pixelWidth * ratio, pixelHeight), Texture2D.whiteTexture);
            }

            // 血条边框文字
            GUI.color = Color.white;
            string hpText = $"{health.CurrentHealth:0}/{health.MaxHealth:0}";
            GUI.Label(bgRect, hpText, barLabelStyle);

            // 警戒值（仅敌人）
            if (perception != null)
            {
                float alert = perception.AlertValue;
                string alertText = alert >= 1f ? "警戒: MAX" : $"警戒: {alert:P0}";
                GUI.color = alertColor;
                var alertRect = new Rect(x, y - 18f, pixelWidth, 16f);
                GUI.Label(alertRect, alertText, alertLabelStyle);
            }

            // 状态文字（敌人 AI 状态）
            if (!string.IsNullOrEmpty(state))
            {
                GUI.color = stateColor;
                var stateRect = new Rect(x, y - 36f, pixelWidth, 18f);
                GUI.Label(stateRect, state, stateLabelStyle);
            }
        }
    }
}
