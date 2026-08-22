using System.Collections.Generic;
using FMBG.AI;
using FMBG.Combat;
using UnityEngine;

namespace FMBG.UI
{
    /// <summary>胜负判定与结果界面：玩家死亡或敌人全灭后显示，R 重开。</summary>
    public sealed class GameResultUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Health playerHealth;
        [SerializeField] private Health[] enemies;

        private readonly List<Health> trackedEnemies = new();
        private GUIStyle titleStyle;
        private GUIStyle hintStyle;
        private bool finished;
        private string resultText;

        private void Awake()
        {
            if (playerHealth != null)
            {
                playerHealth.Died += OnPlayerDied;
            }

            if (enemies != null)
            {
                foreach (var enemy in enemies)
                {
                    RegisterEnemy(enemy);
                }
            }

            RefreshSceneEnemies();
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
            {
                playerHealth.Died -= OnPlayerDied;
            }

            foreach (var enemy in trackedEnemies)
            {
                if (enemy != null)
                {
                    enemy.Died -= OnEnemyDied;
                }
            }
        }

        private void RegisterEnemy(Health enemy)
        {
            if (enemy == null || trackedEnemies.Contains(enemy))
            {
                return;
            }

            trackedEnemies.Add(enemy);
            enemy.Died += OnEnemyDied;
        }

        private void RefreshSceneEnemies()
        {
            foreach (var actor in Object.FindObjectsOfType<EnemyActor>())
            {
                RegisterEnemy(actor != null ? actor.Health : null);
            }
        }

        private void Update()
        {
            if (finished && Input.GetKeyDown(KeyCode.R))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }
        }

        private void OnPlayerDied(DamageInfo info)
        {
            if (!finished)
            {
                finished = true;
                resultText = "你已阵亡\n按 R 重新开始";
            }
        }

        private void OnEnemyDied(DamageInfo info)
        {
            if (finished)
            {
                return;
            }

            // 重新扫描，确保没有手动拖进 enemies 数组的新敌人也参与胜利判定。
            RefreshSceneEnemies();

            bool hasTrackedEnemy = false;
            foreach (var enemy in trackedEnemies)
            {
                if (enemy == null)
                {
                    continue;
                }

                hasTrackedEnemy = true;
                if (enemy.IsAlive)
                {
                    return;
                }
            }

            if (hasTrackedEnemy)
            {
                finished = true;
                resultText = "胜利！敌人已全灭\n按 R 重新开始";
            }
        }

        private void OnGUI()
        {
            if (!finished)
            {
                return;
            }

            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 48,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                hintStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 20,
                    alignment = TextAnchor.MiddleCenter
                };
            }

            float cx = Screen.width * 0.5f;
            float cy = Screen.height * 0.4f;

            GUI.color = new Color(0f, 0f, 0f, 0.6f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(cx - 400, cy - 60, 800, 80), resultText, titleStyle);
        }
    }
}
