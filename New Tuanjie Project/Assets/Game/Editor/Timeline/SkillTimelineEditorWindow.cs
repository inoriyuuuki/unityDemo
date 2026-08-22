using System;
using System.Collections.Generic;
using FMBG.Timeline;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace FMBG.EditorTools
{
    /// <summary>
    /// UI Toolkit 技能时间轴编辑器：取代原 Slate CutsceneEditor。
    /// 支持轨道分组、Clip 拖拽/缩放/增删、播放头预览与 Clip 参数编辑。
    /// </summary>
    public sealed class SkillTimelineEditorWindow : EditorWindow
    {
        private const float GutterWidth = 130f;
        private const float RulerHeight = 28f;
        private const float RowHeight = 40f;
        private const float SnapInterval = 0.01f;
        private const float MinPixelsPerSecond = 30f;
        private const float MaxPixelsPerSecond = 600f;
        private const float DefaultPixelsPerSecond = 150f;

        private static readonly SkillTrackType[] TrackOrder =
        {
            SkillTrackType.Animation,
            SkillTrackType.Gameplay,
            SkillTrackType.Effect,
            SkillTrackType.Audio
        };

        private static readonly Dictionary<SkillTrackType, Color> TrackColors = new()
        {
            { SkillTrackType.Animation, new Color(0.42f, 0.62f, 0.85f) },
            { SkillTrackType.Gameplay, new Color(0.36f, 0.72f, 0.52f) },
            { SkillTrackType.Effect, new Color(0.88f, 0.62f, 0.25f) },
            { SkillTrackType.Audio, new Color(0.66f, 0.46f, 0.78f) }
        };

        private SkillTimeline timeline;
        private SerializedObject serializedTimeline;
        private SkillTimelineClip selectedClip;

        // UI
        private ObjectField timelineField;
        private FloatField lengthField;
        private Slider zoomSlider;
        private Label timeLabel;
        private Button playButton;
        private Button deleteButton;
        private ScrollView timelineScroll;
        private VisualElement content;
        private VisualElement ruler;
        private VisualElement playhead;

        private readonly Dictionary<SkillTimelineClip, VisualElement> clipElements = new();
        private IMGUIContainer inspectorIMGUI;

        // 预览
        private float previewTime;
        private bool previewPlaying;
        private double lastPreviewTime;

        // 拖拽状态
        private enum DragMode
        {
            None,
            Move,
            ResizeLeft,
            ResizeRight,
            Scrub
        }

        private DragMode dragMode = DragMode.None;
        private SkillTimelineClip dragClip;
        private VisualElement dragElement;
        private float dragStartX;
        private float dragStartTime;
        private float dragStartDuration;

        private float PixelsPerSecond => zoomSlider != null ? zoomSlider.value : DefaultPixelsPerSecond;

        [MenuItem("Game/Tools/Skill Timeline Editor")]
        public static void Open()
        {
            SkillTimelineEditorWindow window = GetWindow<SkillTimelineEditorWindow>("Skill Timeline Editor");
            window.minSize = new Vector2(900f, 480f);
            window.Show();
        }

        /// <summary>打开编辑器并加载指定时间轴（供 Inspector / 菜单调用）。</summary>
        public static void OpenOrCreate(SkillTimeline asset)
        {
            SkillTimelineEditorWindow window = GetWindow<SkillTimelineEditorWindow>("Skill Timeline Editor");
            window.minSize = new Vector2(900f, 480f);
            window.LoadTimeline(asset);
            window.Show();
        }

        /// <summary>双击 SkillTimeline 资产时直接打开编辑器。</summary>
        [OnOpenAsset]
        private static bool OnOpenAsset(int instanceID, int line)
        {
            SkillTimeline asset = EditorUtility.InstanceIDToObject(instanceID) as SkillTimeline;
            if (asset == null)
            {
                return false;
            }

            SkillTimelineEditorWindow window = GetWindow<SkillTimelineEditorWindow>("Skill Timeline Editor");
            window.LoadTimeline(asset);
            window.Show();
            return true;
        }

        private void OnEnable()
        {
            BuildUI();
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            StopPreview();
        }

        private void OnSelectionChange()
        {
            if (timeline == null && Selection.activeObject is SkillTimeline asset)
            {
                LoadTimeline(asset);
            }
        }

        private void OnUndoRedo()
        {
            serializedTimeline?.Update();
            RefreshAll();
        }

        private void BuildUI()
        {
            rootVisualElement.Clear();

            VisualElement root = new();
            root.style.flexGrow = 1f;
            root.style.flexDirection = FlexDirection.Column;
            rootVisualElement.Add(root);

            BuildToolbar(root);
            BuildTimelineArea(root);
            BuildInspector(root);
        }

        private void BuildToolbar(VisualElement parent)
        {
            VisualElement toolbar = new();
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.style.paddingLeft = 6f;
            toolbar.style.paddingRight = 6f;
            toolbar.style.paddingTop = 4f;
            toolbar.style.paddingBottom = 4f;
            toolbar.style.borderBottomWidth = 1f;
            toolbar.style.borderBottomColor = new Color(0f, 0f, 0f, 0.4f);
            toolbar.style.flexShrink = 0f;
            parent.Add(toolbar);

            timelineField = new ObjectField("Timeline")
            {
                objectType = typeof(SkillTimeline),
                allowSceneObjects = false
            };
            timelineField.style.width = 320f;
            timelineField.style.marginRight = 12f;
            timelineField.RegisterValueChangedCallback(evt => LoadTimeline(evt.newValue as SkillTimeline));
            toolbar.Add(timelineField);

            Label lengthLabel = new("Length");
            lengthLabel.style.marginLeft = 6f;
            lengthLabel.style.marginRight = 4f;
            lengthLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            toolbar.Add(lengthLabel);

            lengthField = new FloatField();
            lengthField.style.width = 60f;
            lengthField.RegisterValueChangedCallback(evt =>
            {
                if (timeline == null)
                {
                    return;
                }

                Undo.RecordObject(timeline, "Set Timeline Length");
                timeline.Length = Mathf.Max(0.1f, evt.newValue);
                EditorUtility.SetDirty(timeline);
                RefreshTimelineLayout();
            });
            toolbar.Add(lengthField);

            Label zoomLabel = new("Zoom");
            zoomLabel.style.marginLeft = 12f;
            zoomLabel.style.marginRight = 4f;
            toolbar.Add(zoomLabel);

            zoomSlider = new Slider(MinPixelsPerSecond, MaxPixelsPerSecond)
            {
                value = DefaultPixelsPerSecond
            };
            zoomSlider.style.width = 140f;
            zoomSlider.RegisterValueChangedCallback(evt => RefreshTimelineLayout());
            toolbar.Add(zoomSlider);

            playButton = new Button(TogglePreview)
            {
                text = "▶ Preview"
            };
            playButton.style.marginLeft = 12f;
            toolbar.Add(playButton);

            timeLabel = new Label("0.00s");
            timeLabel.style.marginLeft = 8f;
            timeLabel.style.minWidth = 52f;
            timeLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            toolbar.Add(timeLabel);

            Button addButton = new(ShowAddClipMenu)
            {
                text = "+ Add Clip"
            };
            addButton.style.marginLeft = 12f;
            toolbar.Add(addButton);

            deleteButton = new Button(DeleteSelectedClip)
            {
                text = "Delete"
            };
            deleteButton.style.marginLeft = 6f;
            deleteButton.SetEnabled(false);
            toolbar.Add(deleteButton);
        }

        private void BuildTimelineArea(VisualElement parent)
        {
            VisualElement split = new();
            split.style.flexGrow = 1f;
            split.style.flexDirection = FlexDirection.Row;
            parent.Add(split);

            timelineScroll = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            timelineScroll.style.flexGrow = 1f;
            timelineScroll.contentContainer.style.flexGrow = 0f;
            timelineScroll.contentContainer.style.paddingLeft = 0f;
            timelineScroll.contentContainer.style.paddingRight = 0f;
            split.Add(timelineScroll);

            content = new VisualElement();
            content.style.position = Position.Relative;
            content.style.flexGrow = 0f;
            content.style.flexShrink = 0f;
            timelineScroll.contentContainer.Add(content);
        }

        private void BuildInspector(VisualElement parent)
        {
            VisualElement inspectorContainer = new();
            inspectorContainer.style.width = 300f;
            inspectorContainer.style.flexShrink = 0f;
            inspectorContainer.style.borderLeftWidth = 1f;
            inspectorContainer.style.borderLeftColor = new Color(0f, 0f, 0f, 0.4f);
            inspectorContainer.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f, 0.3f);
            parent.Add(inspectorContainer);

            inspectorIMGUI = new IMGUIContainer(OnInspectorGUI);
            inspectorIMGUI.style.flexGrow = 1f;
            inspectorContainer.Add(inspectorIMGUI);
        }

        public void LoadTimeline(SkillTimeline asset)
        {
            StopPreview();

            timeline = asset;
            serializedTimeline = timeline != null ? new SerializedObject(timeline) : null;
            selectedClip = null;
            clipElements.Clear();

            if (timelineField != null)
            {
                timelineField.SetValueWithoutNotify(timeline);
            }

            RefreshAll();
        }

        private void RefreshAll()
        {
            serializedTimeline?.Update();

            if (lengthField != null)
            {
                lengthField.SetValueWithoutNotify(timeline != null ? timeline.Length : 0f);
            }

            if (deleteButton != null)
            {
                deleteButton.SetEnabled(timeline != null && selectedClip != null);
            }

            RefreshTimelineLayout();
            RefreshInspector();
        }

        // ---- 时间轴构建 ----

        private void RefreshTimelineLayout()
        {
            if (content == null)
            {
                return;
            }

            content.Clear();
            clipElements.Clear();
            content.style.height = RulerHeight + TrackOrder.Length * RowHeight;

            if (timeline == null)
            {
                content.style.width = 600f;
                return;
            }

            float viewportWidth = Mathf.Max(600f, timelineScroll.resolvedStyle.width - 16f);
            float contentWidth = Mathf.Max(viewportWidth, GutterWidth + timeline.Length * PixelsPerSecond);
            content.style.width = contentWidth;

            BuildRuler(contentWidth);
            foreach (SkillTrackType trackType in TrackOrder)
            {
                BuildTrackRow(trackType);
            }

            BuildPlayhead();
        }

        private void BuildRuler(float contentWidth)
        {
            ruler = new VisualElement();
            ruler.name = "ruler";
            ruler.style.position = Position.Relative;
            ruler.style.height = RulerHeight;
            ruler.style.borderBottomWidth = 1f;
            ruler.style.borderBottomColor = new Color(0f, 0f, 0f, 0.5f);
            content.Add(ruler);

            float pps = PixelsPerSecond;
            float step = pps >= 300f ? 0.05f : 0.1f;
            float maxTime = Mathf.Max(0.1f, (contentWidth - GutterWidth) / pps);

            for (float t = 0f; t <= maxTime + 0.001f; t += step)
            {
                float x = GutterWidth + t * pps;

                VisualElement tick = new();
                tick.style.position = Position.Absolute;
                tick.style.left = x;
                tick.style.top = 0f;
                tick.style.width = 1f;
                tick.style.height = t <= 0.001f ? RulerHeight : 7f;
                tick.style.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 0.6f);
                ruler.Add(tick);

                Label label = new(t.ToString("0.00"));
                label.style.position = Position.Absolute;
                label.style.left = x + 3f;
                label.style.top = 12f;
                label.style.fontSize = 10f;
                label.style.color = new Color(0.85f, 0.85f, 0.85f);
                ruler.Add(label);
            }

            ruler.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (timeline == null || evt.button != 0)
                {
                    return;
                }

                StopPreview();
                dragMode = DragMode.Scrub;
                ruler.CapturePointer(evt.pointerId);
                UpdateScrub(content.WorldToLocal(evt.position));
                evt.StopPropagation();
            });
            ruler.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (dragMode == DragMode.Scrub)
                {
                    UpdateScrub(content.WorldToLocal(evt.position));
                }
            });
            ruler.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (dragMode == DragMode.Scrub)
                {
                    dragMode = DragMode.None;
                    ruler.ReleasePointer(evt.pointerId);
                }
            });
        }

        private void BuildTrackRow(SkillTrackType trackType)
        {
            VisualElement row = new();
            row.style.flexDirection = FlexDirection.Row;
            row.style.height = RowHeight;
            row.style.borderBottomWidth = 1f;
            row.style.borderBottomColor = new Color(1f, 1f, 1f, 0.06f);
            content.Add(row);

            Label gutter = new(trackType.ToString());
            gutter.style.width = GutterWidth;
            gutter.style.flexShrink = 0f;
            gutter.style.paddingLeft = 8f;
            gutter.style.unityTextAlign = TextAnchor.MiddleLeft;
            gutter.style.unityFontStyleAndWeight = FontStyle.Bold;
            gutter.style.fontSize = 12f;
            gutter.style.color = TrackColors[trackType];
            row.Add(gutter);

            VisualElement clipArea = new();
            clipArea.style.flexGrow = 1f;
            clipArea.style.position = Position.Relative;
            row.Add(clipArea);

            List<SkillTimelineClip> clips = timeline.GetClipsByTrack(trackType);
            foreach (SkillTimelineClip clip in clips)
            {
                VisualElement element = CreateClipElement(clip, trackType);
                clipArea.Add(element);
                clipElements[clip] = element;
            }

            // 双击轨道空白 = 在双击处添加 Clip
            clipArea.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (timeline != null && evt.clickCount == 2 && dragMode == DragMode.None)
                {
                    AddClipAt(content.WorldToLocal(evt.position).x);
                }
            });
        }

        private VisualElement CreateClipElement(SkillTimelineClip clip, SkillTrackType trackType)
        {
            VisualElement element = new();
            element.name = "clip";
            element.userData = clip;
            element.style.position = Position.Absolute;
            element.style.top = 3f;
            element.style.bottom = 3f;
            element.style.borderTopLeftRadius = 3f;
            element.style.borderBottomLeftRadius = 3f;
            element.style.borderTopRightRadius = 3f;
            element.style.borderBottomRightRadius = 3f;
            element.style.borderLeftWidth = 2f;
            element.style.borderRightWidth = 2f;
            element.style.borderLeftColor = new Color(1f, 1f, 1f, 0.45f);
            element.style.borderRightColor = new Color(1f, 1f, 1f, 0.45f);
            element.style.overflow = Overflow.Hidden;
            element.style.backgroundColor = TrackColors[trackType] * 0.82f;
            element.tooltip = "中间拖动平移；左右边缘拖动修改起止时间";

            Label label = new(clip.Info);
            label.style.position = Position.Absolute;
            label.style.left = 12f;
            label.style.right = 12f;
            label.style.top = 0f;
            label.style.bottom = 0f;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.fontSize = 11f;
            label.style.color = new Color(1f, 1f, 1f, 0.95f);
            label.style.textOverflow = TextOverflow.Ellipsis;
            label.style.whiteSpace = WhiteSpace.NoWrap;
            element.Add(label);

            VisualElement leftHandle = CreateResizeHandle("left");
            VisualElement rightHandle = CreateResizeHandle("right");
            element.Add(leftHandle);
            element.Add(rightHandle);

            element.RegisterCallback<PointerDownEvent>(evt =>
                OnClipPointerDown(evt, clip, element, DragMode.Move));
            element.RegisterCallback<PointerMoveEvent>(OnClipPointerMove);
            element.RegisterCallback<PointerUpEvent>(OnClipPointerUp);

            leftHandle.RegisterCallback<PointerDownEvent>(evt =>
                OnClipPointerDown(evt, clip, element, DragMode.ResizeLeft));
            rightHandle.RegisterCallback<PointerDownEvent>(evt =>
                OnClipPointerDown(evt, clip, element, DragMode.ResizeRight));
            leftHandle.RegisterCallback<PointerMoveEvent>(OnClipPointerMove);
            rightHandle.RegisterCallback<PointerMoveEvent>(OnClipPointerMove);
            leftHandle.RegisterCallback<PointerUpEvent>(OnClipPointerUp);
            rightHandle.RegisterCallback<PointerUpEvent>(OnClipPointerUp);

            element.RegisterCallback<ContextClickEvent>(evt =>
            {
                SelectClip(clip);
                ShowClipContextMenu(clip, evt.mousePosition);
                evt.StopPropagation();
            });

            LayoutClipElement(clip, element);
            ApplyClipStyle(element, clip == selectedClip, IsClipActiveAt(clip, previewTime));
            return element;
        }

        private static VisualElement CreateResizeHandle(string side)
        {
            VisualElement handle = new();
            handle.name = side + "Handle";
            handle.style.position = Position.Absolute;
            handle.style.top = 0f;
            handle.style.bottom = 0f;
            handle.style.width = 10f;
            if (side == "left")
            {
                handle.style.left = 0f;
                handle.style.borderRightWidth = 1f;
                handle.style.borderRightColor = new Color(0f, 0f, 0f, 0.25f);
                handle.tooltip = "拖动修改开始时间";
            }
            else
            {
                handle.style.right = 0f;
                handle.style.borderLeftWidth = 1f;
                handle.style.borderLeftColor = new Color(0f, 0f, 0f, 0.25f);
                handle.tooltip = "拖动修改结束时间";
            }

            handle.style.backgroundColor = new Color(1f, 1f, 1f, 0.28f);
            return handle;
        }

        private void LayoutClipElement(SkillTimelineClip clip, VisualElement element)
        {
            float pps = PixelsPerSecond;
            float width = Mathf.Max(6f, clip.Duration * pps);
            element.style.left = clip.StartTime * pps;
            element.style.width = width;

            // 片段太短时不显示左右手柄，整体作为平移区域；变宽后手柄自动出现
            bool showHandles = width >= 18f;
            VisualElement leftHandle = element.Q("leftHandle");
            VisualElement rightHandle = element.Q("rightHandle");
            if (leftHandle != null)
            {
                leftHandle.style.display = showHandles ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (rightHandle != null)
            {
                rightHandle.style.display = showHandles ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void ApplyClipStyle(VisualElement element, bool selected, bool active)
        {
            element.style.borderTopWidth = selected ? 2f : 0f;
            element.style.borderBottomWidth = selected ? 2f : 0f;
            element.style.borderTopColor = Color.white;
            element.style.borderBottomColor = Color.white;
            element.style.borderLeftColor = selected ? Color.white : new Color(1f, 1f, 1f, 0.45f);
            element.style.borderRightColor = selected ? Color.white : new Color(1f, 1f, 1f, 0.45f);
            element.style.opacity = active ? 1f : 0.55f;
        }

        private void BuildPlayhead()
        {
            playhead = new VisualElement();
            playhead.name = "playhead";
            playhead.style.position = Position.Absolute;
            playhead.style.top = 0f;
            playhead.style.bottom = 0f;
            playhead.style.width = 2f;
            playhead.style.backgroundColor = new Color(1f, 0.35f, 0.2f);
            playhead.pickingMode = PickingMode.Ignore;
            content.Add(playhead);

            UpdatePlayheadPosition();
        }

        private void UpdatePlayheadPosition()
        {
            if (playhead == null || timeline == null)
            {
                return;
            }

            float x = GutterWidth + Mathf.Clamp(previewTime, 0f, timeline.Length) * PixelsPerSecond;
            playhead.style.left = x;
            if (timeLabel != null)
            {
                timeLabel.text = previewTime.ToString("0.00") + "s";
            }
        }

        // ---- 交互 ----

        private void OnClipPointerDown(PointerDownEvent evt, SkillTimelineClip clip, VisualElement element, DragMode mode)
        {
            if (timeline == null || evt.button != 0)
            {
                return;
            }

            evt.StopPropagation();
            SelectClip(clip);

            dragMode = mode;
            dragClip = clip;
            dragElement = element;
            dragStartX = content.WorldToLocal(evt.position).x;
            dragStartTime = clip.StartTime;
            dragStartDuration = clip.Duration;
            Undo.RecordObject(timeline, "Edit Clip");
            element.CapturePointer(evt.pointerId);
        }

        private void OnClipPointerMove(PointerMoveEvent evt)
        {
            if (dragMode == DragMode.None || dragClip == null || timeline == null)
            {
                return;
            }

            float pps = PixelsPerSecond;
            float pointerX = content.WorldToLocal(evt.position).x;
            float deltaSeconds = (pointerX - dragStartX) / pps;

            if (dragMode == DragMode.Move)
            {
                dragClip.StartTime = Snap(dragStartTime + deltaSeconds);
            }
            else if (dragMode == DragMode.ResizeRight)
            {
                dragClip.Duration = Mathf.Max(SnapInterval, Snap(dragStartDuration + deltaSeconds));
            }
            else if (dragMode == DragMode.ResizeLeft)
            {
                float newStart = Snap(dragStartTime + deltaSeconds);
                float end = dragStartTime + dragStartDuration;
                if (end - newStart >= SnapInterval)
                {
                    dragClip.StartTime = newStart;
                    dragClip.Duration = end - newStart;
                }
            }

            if (dragElement != null)
            {
                LayoutClipElement(dragClip, dragElement);
            }

            EditorUtility.SetDirty(timeline);
            RefreshInspector();
            evt.StopPropagation();
        }

        private void OnClipPointerUp(PointerUpEvent evt)
        {
            if (dragMode == DragMode.None)
            {
                return;
            }

            dragMode = DragMode.None;
            dragClip = null;
            dragElement = null;

            if (timeline != null)
            {
                timeline.EnsureLengthCoversContent();
                EditorUtility.SetDirty(timeline);
                lengthField.SetValueWithoutNotify(timeline.Length);
                RefreshTimelineLayout();
            }

            evt.StopPropagation();
        }

        private void UpdateScrub(Vector2 localPosition)
        {
            float pps = PixelsPerSecond;
            float seconds = (localPosition.x - GutterWidth) / pps;
            previewTime = Mathf.Clamp(Snap(seconds), 0f, Mathf.Max(0.1f, timeline.Length));
            UpdatePlayheadPosition();
            UpdateActiveHighlights();
        }

        private void UpdateActiveHighlights()
        {
            foreach (KeyValuePair<SkillTimelineClip, VisualElement> pair in clipElements)
            {
                bool active = IsClipActiveAt(pair.Key, previewTime);
                pair.Value.style.opacity = active ? 1f : 0.55f;
            }
        }

        private void SelectClip(SkillTimelineClip clip)
        {
            if (selectedClip == clip)
            {
                return;
            }

            selectedClip = clip;
            deleteButton.SetEnabled(clip != null);
            UpdateSelectionStyles();
            RefreshInspector();
        }

        private void UpdateSelectionStyles()
        {
            foreach (KeyValuePair<SkillTimelineClip, VisualElement> pair in clipElements)
            {
                bool isSelected = pair.Key == selectedClip;
                pair.Value.style.borderTopWidth = isSelected ? 2f : 0f;
                pair.Value.style.borderBottomWidth = isSelected ? 2f : 0f;
                pair.Value.style.borderTopColor = Color.white;
                pair.Value.style.borderBottomColor = Color.white;
                pair.Value.style.borderLeftColor = isSelected ? Color.white : new Color(1f, 1f, 1f, 0.45f);
                pair.Value.style.borderRightColor = isSelected ? Color.white : new Color(1f, 1f, 1f, 0.45f);
            }
        }

        private void DeleteSelectedClip()
        {
            if (timeline == null || selectedClip == null)
            {
                return;
            }

            SkillTimelineClip clip = selectedClip;
            Undo.RecordObject(timeline, "Delete Clip");
            timeline.RemoveClip(clip);
            selectedClip = null;
            timeline.EnsureLengthCoversContent();
            EditorUtility.SetDirty(timeline);
            RefreshAll();
        }

        private void ShowClipContextMenu(SkillTimelineClip clip, Vector2 mousePosition)
        {
            GenericMenu menu = new();
            menu.AddItem(new GUIContent("Duplicate"), false, () => DuplicateClip(clip));
            menu.AddItem(new GUIContent("Delete"), false, DeleteSelectedClip);
            menu.AddSeparator(string.Empty);
            foreach (SkillTrackType track in TrackOrder)
            {
                bool isCurrent = clip.TrackType == track;
                menu.AddItem(
                    new GUIContent("Move to Track/" + track),
                    isCurrent,
                    () => MoveClipToTrack(clip, track));
            }

            menu.ShowAsContext();
        }

        private void MoveClipToTrack(SkillTimelineClip clip, SkillTrackType track)
        {
            if (timeline == null || clip == null || clip.TrackType == track)
            {
                return;
            }

            Undo.RecordObject(timeline, "Move Clip to Track");
            clip.TrackType = track;
            EditorUtility.SetDirty(timeline);
            RefreshAll();
        }

        private void DuplicateClip(SkillTimelineClip source)
        {
            if (timeline == null || source == null)
            {
                return;
            }

            SkillTimelineClip copy = (SkillTimelineClip)JsonUtility.FromJson(
                JsonUtility.ToJson(source),
                source.GetType());
            copy.StartTime = Snap(source.StartTime + 0.1f);

            Undo.RecordObject(timeline, "Duplicate Clip");
            timeline.AddClip(copy);
            selectedClip = copy;
            EditorUtility.SetDirty(timeline);
            RefreshAll();
        }

        private void ShowAddClipMenu()
        {
            GenericMenu menu = new();
            foreach ((Type type, string label) in GetClipTypes())
            {
                menu.AddItem(new GUIContent(label), false, () => AddClipOfType(type));
            }

            menu.ShowAsContext();
        }

        private static List<(Type, string)> GetClipTypes()
        {
            return new List<(Type, string)>
            {
                (typeof(AttackAnimationClip), "Animation/Attack Animation"),
                (typeof(FaceTargetClip), "Animation/Face Target"),
                (typeof(LockMovementClip), "Gameplay/Lock Movement"),
                (typeof(DashClip), "Gameplay/Dash"),
                (typeof(MeleeHitWindowClip), "Gameplay/Melee Hit Window"),
                (typeof(SpawnProjectileClip), "Gameplay/Spawn Projectile"),
                (typeof(SkillSignalClip), "Gameplay/Skill Signal"),
                (typeof(SpawnEffectClip), "Effect/Spawn Effect"),
                (typeof(PlaySkillAudioClip), "Audio/Play Audio")
            };
        }

        private void AddClipAt(float pointerX)
        {
            float seconds = (pointerX - GutterWidth) / PixelsPerSecond;
            previewTime = Mathf.Max(0f, Snap(seconds));
            ShowAddClipMenu();
        }

        private void AddClipOfType(Type clipType)
        {
            if (timeline == null)
            {
                return;
            }

            SkillTimelineClip clip = (SkillTimelineClip)Activator.CreateInstance(clipType);
            clip.StartTime = Snap(previewTime);
            clip.Duration = DefaultDurationFor(clip);

            Undo.RecordObject(timeline, "Add Clip");
            timeline.AddClip(clip);
            timeline.EnsureLengthCoversContent();
            selectedClip = clip;
            EditorUtility.SetDirty(timeline);
            RefreshAll();
        }

        private static float DefaultDurationFor(SkillTimelineClip clip)
        {
            return clip switch
            {
                AttackAnimationClip => 0.4f,
                DashClip => 0.25f,
                MeleeHitWindowClip => 0.15f,
                FaceTargetClip => 0.2f,
                _ => 0.01f
            };
        }

        private static float Snap(float value)
        {
            return Mathf.Round(value / SnapInterval) * SnapInterval;
        }

        private static bool IsClipActiveAt(SkillTimelineClip clip, float time)
        {
            return clip.StartTime <= time && time < clip.EndTime;
        }

        // ---- 预览 ----

        private void TogglePreview()
        {
            if (timeline == null)
            {
                return;
            }

            if (previewPlaying)
            {
                StopPreview();
                return;
            }

            previewPlaying = true;
            previewTime = 0f;
            lastPreviewTime = EditorApplication.timeSinceStartup;
            EditorApplication.update += PreviewUpdate;
            playButton.text = "■ Stop";
            UpdatePlayheadPosition();
        }

        private void StopPreview()
        {
            if (!previewPlaying)
            {
                return;
            }

            previewPlaying = false;
            EditorApplication.update -= PreviewUpdate;
            playButton.text = "▶ Preview";
            previewTime = 0f;
            UpdatePlayheadPosition();
            UpdateActiveHighlights();
        }

        private void PreviewUpdate()
        {
            if (timeline == null)
            {
                StopPreview();
                return;
            }

            double now = EditorApplication.timeSinceStartup;
            float delta = (float)(now - lastPreviewTime);
            lastPreviewTime = now;

            float length = Mathf.Max(timeline.Length, timeline.ContentLength);
            previewTime += delta;
            if (previewTime >= length)
            {
                previewTime = 0f;
            }

            UpdatePlayheadPosition();
            UpdateActiveHighlights();
        }

        // ---- 属性面板 ----

        private void RefreshInspector()
        {
            inspectorIMGUI?.MarkDirtyRepaint();
        }

        private void OnInspectorGUI()
        {
            if (timeline == null)
            {
                EditorGUILayout.HelpBox("请选择一个 SkillTimeline 资产。", MessageType.Info);
                return;
            }

            serializedTimeline?.Update();

            EditorGUILayout.LabelField("Timeline: " + timeline.name, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                $"Clips: {timeline.Clips.Count}    Content Length: {timeline.ContentLength:0.00}s",
                EditorStyles.miniLabel);
            EditorGUILayout.Space();

            if (selectedClip == null)
            {
                EditorGUILayout.HelpBox("在时间轴上选择 / 添加一个 Clip 以编辑参数。", MessageType.Info);
                return;
            }

            int index = IndexOfClip(selectedClip);
            if (index < 0)
            {
                EditorGUILayout.HelpBox("所选 Clip 已不存在。", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField("Clip: " + selectedClip.GetType().Name, EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();

            SerializedProperty listProperty = serializedTimeline.FindProperty("clips");
            SerializedProperty clipProperty = listProperty.GetArrayElementAtIndex(index);
            EditorGUILayout.PropertyField(clipProperty, new GUIContent("Clip"), true);

            if (EditorGUI.EndChangeCheck() && serializedTimeline.hasModifiedProperties)
            {
                serializedTimeline.ApplyModifiedProperties();
                EditorUtility.SetDirty(timeline);
                timeline.EnsureLengthCoversContent();
                lengthField.SetValueWithoutNotify(timeline.Length);
                RefreshTimelineLayout();
            }
        }

        private int IndexOfClip(SkillTimelineClip clip)
        {
            if (timeline == null)
            {
                return -1;
            }

            IReadOnlyList<SkillTimelineClip> clips = timeline.Clips;
            for (int i = 0; i < clips.Count; i++)
            {
                if (ReferenceEquals(clips[i], clip))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
