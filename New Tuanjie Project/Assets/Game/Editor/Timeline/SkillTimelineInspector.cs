using FMBG.Timeline;
using UnityEditor;
using UnityEngine;

namespace FMBG.EditorTools
{
    /// <summary>
    /// SkillTimeline 资产 Inspector：保留默认可编辑字段（length + clips 多态列表），
    /// 并提供入口按钮打开 UI Toolkit 时间轴编辑器。
    /// </summary>
    [CustomEditor(typeof(SkillTimeline))]
    public sealed class SkillTimelineInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SkillTimeline timeline = (SkillTimeline)target;

            EditorGUILayout.HelpBox(
                "技能时间轴可由 UI Toolkit 编辑器（Game/Tools/Skill Timeline Editor）拖拽编辑，也可在下方直接修改数值。",
                MessageType.Info);

            if (GUILayout.Button("Open Skill Timeline Editor"))
            {
                SkillTimelineEditorWindow.OpenOrCreate(timeline);
            }

            EditorGUILayout.Space();

            // 渲染默认可编辑字段：length + clips（SerializeReference 多态列表）
            DrawDefaultInspector();

            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(timeline);
            }
        }
    }
}
