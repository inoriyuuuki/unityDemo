using System;
using System.Collections.Generic;
using FMBG.Timeline;
using UnityEngine;

namespace FMBG.Skills
{
    /// <summary>
    /// 技能时间轴播放器：驱动 <see cref="SkillTimeline"/> 中的 Clip 按时间执行。
    /// 取代原 Slate 适配层（SlateSkillPlayer），隔离时间轴运行时 API。
    /// </summary>
    public sealed class SkillTimelinePlayer : MonoBehaviour
    {
        private SkillTimeline timeline;
        private SkillExecutionContext currentContext;
        private Transform currentActor;
        private Action currentCallback;
        private float playTime;
        private float duration;
        private bool isPlaying;

        private readonly HashSet<SkillTimelineClip> enteredClips = new();
        private readonly HashSet<SkillTimelineClip> activeClips = new();

        public bool IsPlaying => isPlaying;

        public SkillExecutionContext CurrentContext => currentContext;

        /// <summary>播放技能时间轴（施法者作为 Actor 绑定）。</summary>
        public void Play(SkillExecutionContext context, Action onFinished)
        {
            Stop();

            currentContext = context;
            currentCallback = onFinished;

            if (context == null || context.Skill == null || context.Skill.Timeline == null)
            {
                Debug.LogWarning("SkillConfig 或 Timeline 为空。", this);
                Finish();
                return;
            }

            currentActor = context.Caster != null ? context.Caster.transform : transform;
            StartTimeline(context.Skill.Timeline);
        }

        /// <summary>直接播放时间轴（编辑器预览/测试用，不依赖 SkillConfig）。</summary>
        public void PlayTimeline(SkillTimeline targetTimeline, Transform actor, Action onFinished)
        {
            Stop();

            currentContext = null;
            currentActor = actor;
            currentCallback = onFinished;

            if (targetTimeline == null)
            {
                Finish();
                return;
            }

            StartTimeline(targetTimeline);
        }

        public void Stop()
        {
            isPlaying = false;

            // 停止/结束时：先退出仍活动的 Clip，再对所有进入过的 Clip 执行反向恢复。
            if (timeline != null)
            {
                SkillClipContext context = new(currentContext, currentActor);
                foreach (SkillTimelineClip clip in timeline.Clips)
                {
                    if (clip == null || !enteredClips.Contains(clip))
                    {
                        continue;
                    }

                    if (activeClips.Contains(clip))
                    {
                        clip.OnExit(context);
                    }

                    clip.OnReverse(context);
                }
            }

            enteredClips.Clear();
            activeClips.Clear();
            timeline = null;
            currentContext = null;
            currentActor = null;
            currentCallback = null;
            playTime = 0f;
        }

        /// <summary>推进一帧（编辑器预览/单元测试可显式驱动）。</summary>
        public void Tick(float deltaTime)
        {
            if (!isPlaying || timeline == null)
            {
                return;
            }

            float nextTime = Mathf.Min(duration, playTime + deltaTime);
            SkillClipContext context = new(currentContext, currentActor);

            foreach (SkillTimelineClip clip in timeline.Clips)
            {
                if (clip == null || enteredClips.Contains(clip))
                {
                    continue;
                }

                if (clip.StartTime <= nextTime)
                {
                    enteredClips.Add(clip);
                    activeClips.Add(clip);
                    clip.OnEnter(context);
                }
            }

            foreach (SkillTimelineClip clip in timeline.Clips)
            {
                if (clip == null || !activeClips.Contains(clip))
                {
                    continue;
                }

                if (clip.EndTime <= nextTime)
                {
                    activeClips.Remove(clip);
                    clip.OnExit(context);
                }
                else
                {
                    clip.OnUpdate(context, Mathf.Max(0f, nextTime - clip.StartTime));
                }
            }

            playTime = nextTime;
            if (playTime >= duration)
            {
                Finish();
            }
        }

        private void StartTimeline(SkillTimeline targetTimeline)
        {
            timeline = targetTimeline;
            duration = Mathf.Max(targetTimeline.Length, targetTimeline.ContentLength);
            playTime = 0f;
            isPlaying = true;
            enteredClips.Clear();
            activeClips.Clear();

            // 首帧处理 startTime == 0 的 Clip
            Tick(0f);
        }

        private void Update()
        {
            if (isPlaying)
            {
                Tick(Time.deltaTime);
            }
        }

        private void Finish()
        {
            Action callback = currentCallback;
            Stop();
            callback?.Invoke();
        }

        private void OnDestroy()
        {
            Stop();
        }
    }
}
