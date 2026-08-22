using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FMBG.AI
{
    /// <summary>编辑器校验器：检查状态图配置错误（数据模型自校验）。</summary>
    public static class EnemyGraphValidator
    {
        [MenuItem("Game/AI/Validate Enemy State Graph")]
        public static void ValidateSelected()
        {
            foreach (Object o in Selection.objects)
            {
                if (o is EnemyStateGraph graph)
                {
                    ValidateAndLog(graph);
                }
            }
        }

        public static List<string> Validate(EnemyStateGraph graph)
        {
            return graph != null
                ? graph.GetValidationErrors()
                : new List<string> { "图为空。" };
        }

        private static void ValidateAndLog(EnemyStateGraph graph)
        {
            List<string> errors = Validate(graph);
            if (errors.Count == 0)
            {
                Debug.Log($"[GraphValidator] {graph.name} 校验通过。", graph);
            }
            else
            {
                Debug.LogError($"[GraphValidator] {graph.name} 存在 {errors.Count} 个问题:\n" +
                               string.Join("\n", errors.Select(e => " - " + e)), graph);
            }
        }
    }
}
