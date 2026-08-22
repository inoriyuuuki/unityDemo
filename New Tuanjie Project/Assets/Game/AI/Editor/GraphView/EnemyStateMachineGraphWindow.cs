using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace FMBG.AI.Editor
{
    /// <summary>GraphView 状态机编辑器窗口：双击 .asset 或通过 Inspector 打开。</summary>
    public sealed class EnemyStateMachineGraphWindow : EditorWindow
    {
        [SerializeField] private EnemyStateGraph currentGraph;
        private EnemyStateMachineGraphView graphView;

        [MenuItem("Game/AI/Enemy State Graph (GraphView)")]
        public static void OpenWindow()
        {
            var window = GetWindow<EnemyStateMachineGraphWindow>("Enemy State Graph");
            window.Show();
        }

        public static void Open(EnemyStateGraph graph)
        {
            var window = GetWindow<EnemyStateMachineGraphWindow>("Enemy State Graph");
            window.LoadGraph(graph);
            window.Show();
        }

        [OnOpenAsset]
        private static bool OnOpenAsset(int instanceID, int line)
        {
            if (EditorUtility.InstanceIDToObject(instanceID) is EnemyStateGraph graph)
            {
                Open(graph);
                return true;
            }
            return false;
        }

        private void OnEnable()
        {
            rootVisualElement.Clear();

            var toolbar = new UnityEditor.UIElements.Toolbar();
            var title = new Label("敌人状态机 GraphView") { style = { unityFontStyleAndWeight = FontStyle.Bold } };
            toolbar.Add(title);

            var saveButton = new Button(() => graphView?.Save()) { text = "保存" };
            toolbar.Add(saveButton);

            var validateButton = new Button(ValidateCurrent) { text = "校验" };
            toolbar.Add(validateButton);

            rootVisualElement.Add(toolbar);

            graphView = new EnemyStateMachineGraphView();
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);

            if (currentGraph != null)
            {
                graphView.LoadGraph(currentGraph);
            }
        }

        public void LoadGraph(EnemyStateGraph graph)
        {
            currentGraph = graph;
            if (graphView != null)
            {
                graphView.LoadGraph(graph);
            }
            titleContent = new GUIContent(graph != null ? $"Enemy State Graph - {graph.name}" : "Enemy State Graph");
        }

        private void ValidateCurrent()
        {
            if (currentGraph == null)
            {
                return;
            }
            var errors = currentGraph.GetValidationErrors();
            if (errors.Count == 0)
            {
                Debug.Log($"[GraphView] {currentGraph.name} 校验通过。", currentGraph);
            }
            else
            {
                Debug.LogError($"[GraphView] {currentGraph.name} 存在 {errors.Count} 个问题:\n" +
                               string.Join("\n", errors.ConvertAll(e => " - " + e)), currentGraph);
            }
        }
    }
}
