using System.Collections.Generic;
using UnityEngine;

namespace FMBG.Timeline
{
    /// <summary>
    /// 技能时间轴资源：持有按时间排列的 Clip 列表，由 UI Toolkit 编辑器创建/编辑。
    /// 取代原 Slate Cutscene prefab 承担"技能时序"配置职责。
    /// </summary>
    [CreateAssetMenu(
        fileName = "SkillTimeline",
        menuName = "Game/Combat/Skill Timeline")]
    public sealed class SkillTimeline : ScriptableObject
    {
        [SerializeField] private float length = 1f;
        [SerializeReference] private List<SkillTimelineClip> clips = new();

        /// <summary>编辑器展示总长度（播放时取 Length 与内容结束时间的较大值）。</summary>
        public float Length
        {
            get => length;
            set => length = Mathf.Max(0f, value);
        }

        public IReadOnlyList<SkillTimelineClip> Clips => clips;

        /// <summary>内容实际结束时间（最后一个 Clip 的 EndTime，至少 0.1s）。</summary>
        public float ContentLength
        {
            get
            {
                float maxEnd = 0f;
                foreach (SkillTimelineClip clip in clips)
                {
                    if (clip != null)
                    {
                        maxEnd = Mathf.Max(maxEnd, clip.EndTime);
                    }
                }

                return Mathf.Max(0.1f, maxEnd);
            }
        }

        public void AddClip(SkillTimelineClip clip)
        {
            if (clip == null)
            {
                return;
            }

            clips.Add(clip);
            length = Mathf.Max(length, clip.EndTime);
        }

        public void RemoveClip(SkillTimelineClip clip)
        {
            clips.Remove(clip);
        }

        public void ClearClips()
        {
            clips.Clear();
        }

        /// <summary>按 StartTime 排序（编辑器删除/调整后调用）。</summary>
        public void SortClips()
        {
            clips.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
        }

        /// <summary>确保 Length 覆盖内容，避免播放提前结束。</summary>
        public void EnsureLengthCoversContent()
        {
            length = Mathf.Max(length, ContentLength);
        }

        /// <summary>获取指定轨道上的全部 Clip（按开始时间排序）。</summary>
        public List<SkillTimelineClip> GetClipsByTrack(SkillTrackType trackType)
        {
            List<SkillTimelineClip> result = new();
            foreach (SkillTimelineClip clip in clips)
            {
                if (clip != null && clip.TrackType == trackType)
                {
                    result.Add(clip);
                }
            }

            result.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
            return result;
        }
    }
}
