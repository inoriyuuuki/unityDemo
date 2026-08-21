using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XNode;

namespace FMBG.AI
{
    /// <summary>编辑器校验器：检查状态图配置错误。</summary>
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
            var errors = new List<string>();
            if (graph == null)
            {
                errors.Add("图为空。");
                return errors;
            }

            var entries = graph.nodes.OfType<EnemyEntryNode>().ToList();
            if (entries.Count == 0)
            {
                errors.Add("缺少 Entry 节点。");
            }
            else if (entries.Count > 1)
            {
                errors.Add("存在多个 Entry 节点。");
            }
            else if (entries[0].GetStartState() == null)
            {
                errors.Add("Entry 未连接初始状态。");
            }

            var anyState = graph.nodes.OfType<EnemyAnyStateNode>().ToList();
            if (anyState.Count == 0)
            {
                errors.Add("缺少 Any State 节点（无法处理死亡等全局转换）。");
            }

            var stateNodes = graph.nodes.OfType<EnemyStateNode>().ToList();
            if (stateNodes.Count == 0)
            {
                errors.Add("图中没有状态节点。");
            }

            // 每个状态节点的输出端口至少连接一个目标
            foreach (var node in stateNodes)
            {
                foreach (NodePort port in node.Outputs)
                {
                    if (port.ConnectionCount == 0)
                    {
                        errors.Add($"{node.name} 的输出端口 {port.fieldName} 未连接。");
                    }
                    else if (port.ConnectionCount > 1)
                    {
                        errors.Add($"{node.name} 的输出端口 {port.fieldName} 连接了多个目标。");
                    }
                }
            }

            return errors;
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
