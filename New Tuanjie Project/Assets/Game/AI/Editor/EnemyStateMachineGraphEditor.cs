using System.Text;
using UnityEditor;
using UnityEngine;

namespace FMBG.AI.Editor
{
    /// <summary>EnemyStateGraph 自定义 Inspector：打开 GraphView 编辑器 + 实时校验。</summary>
    [CustomEditor(typeof(EnemyStateGraph))]
    public sealed class EnemyStateMachineGraphEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var graph = (EnemyStateGraph)target;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("打开 GraphView 编辑器", GUILayout.Height(28f)))
            {
                EnemyStateMachineGraphWindow.Open(graph);
            }
            if (GUILayout.Button("校验", GUILayout.Height(28f)))
            {
                var errors = graph.GetValidationErrors();
                if (errors.Count == 0)
                {
                    Debug.Log($"[GraphValidator] {graph.name} 校验通过。", graph);
                }
                else
                {
                    Debug.LogError($"[GraphValidator] {graph.name} 存在 {errors.Count} 个问题:\n" +
                                   string.Join("\n", errors), graph);
                }
            }
            EditorGUILayout.EndHorizontal();

            var validation = graph.GetValidationErrors();
            if (validation.Count == 0)
            {
                EditorGUILayout.HelpBox("校验通过", MessageType.Info);
            }
            else
            {
                var sb = new StringBuilder();
                foreach (string error in validation)
                {
                    sb.AppendLine("• " + error);
                }
                EditorGUILayout.HelpBox(sb.ToString(), MessageType.Error);
            }

            EditorGUILayout.Space();
            DrawDefaultInspector();
        }
    }
}
