using System;
using UnityEngine;

namespace FMBG.Timeline
{
    /// <summary>
    /// 技能时间轴 Clip 基类：共有的时间/轨道信息 + 播放生命周期回调。
    /// 具体类型通过 [SerializeReference] 保存在 <see cref="SkillTimeline"/> 中。
    /// </summary>
    [Serializable]
    public abstract class SkillTimelineClip
    {
        [SerializeField] private string clipName = "";
        [SerializeField] private float startTime;
        [SerializeField] private float duration = 0.1f;
        [SerializeField] private SkillTrackType trackType = SkillTrackType.Gameplay;

        protected SkillTimelineClip()
        {
            trackType = DefaultTrackType;
        }

        /// <summary>Clip 显示名（为空时使用类型默认名）。</summary>
        public string ClipName
        {
            get => string.IsNullOrEmpty(clipName) ? DefaultName : clipName;
            set => clipName = value;
        }

        public float StartTime
        {
            get => startTime;
            set => startTime = Mathf.Max(0f, value);
        }

        public float Duration
        {
            get => duration;
            set => duration = Mathf.Max(0f, value);
        }

        public float EndTime => startTime + duration;

        /// <summary>所属轨道（可在编辑器中修改，决定显示在哪一行）。</summary>
        public SkillTrackType TrackType
        {
            get => trackType;
            set => trackType = value;
        }

        /// <summary>派生类型默认轨道（新建实例时使用）。</summary>
        protected virtual SkillTrackType DefaultTrackType => SkillTrackType.Gameplay;

        /// <summary>类型默认显示名。</summary>
        public abstract string DefaultName { get; }

        /// <summary>轨道/列表中的简短描述。</summary>
        public virtual string Info => DefaultName;

        /// <summary>进入 Clip（startTime 到达时调用一次）。</summary>
        public virtual void OnEnter(SkillClipContext context)
        {
        }

        /// <summary>Clip 播放中每帧调用，localTime 为 Clip 内本地时间。</summary>
        public virtual void OnUpdate(SkillClipContext context, float localTime)
        {
        }

        /// <summary>离开 Clip（EndTime 到达时调用一次）。</summary>
        public virtual void OnExit(SkillClipContext context)
        {
        }

        /// <summary>反向/停止时调用，用于恢复进入时修改的状态。</summary>
        public virtual void OnReverse(SkillClipContext context)
        {
        }
    }
}
