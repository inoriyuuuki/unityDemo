using Slate;
using UnityEngine;

namespace FMBG.Skills
{
    /// <summary>Slate 适配层：隔离 Slate API，负责实例化时间轴、播放/停止。</summary>
    public sealed class SlateSkillPlayer : MonoBehaviour
    {
        [SerializeField] private Transform timelineParent;

        private Cutscene currentTimeline;
        private SkillExecutionContext currentContext;
        private System.Action currentCallback;

        public bool IsPlaying => currentTimeline != null && currentTimeline.isActive;

        public SkillExecutionContext CurrentContext => currentContext;

        public void Play(
            SkillExecutionContext context,
            System.Action onFinished)
        {
            Stop();

            currentContext = context;
            currentCallback = onFinished;

            if (context.Skill == null || context.Skill.TimelinePrefab == null)
            {
                Debug.LogWarning("SkillConfig 或 TimelinePrefab 为空。", this);
                Finish();
                return;
            }

            Transform parent = timelineParent != null ? timelineParent : transform;
            currentTimeline = Instantiate(context.Skill.TimelinePrefab, parent);
            currentTimeline.name = context.Skill.DisplayName + "_Instance";

            // 技能时间轴不接管镜头：移除 CameraTrack（避免镜头被锁定）
            DisableCameraTracks(currentTimeline);

            // 绑定施法者为 ActorGroup 的 actor
            if (context.Caster != null)
            {
                var groups = currentTimeline.GetComponentsInChildren<ActorGroup>(true);
                foreach (var group in groups)
                {
                    group.actor = context.Caster.gameObject;
                }
            }

            // Actor 在运行时绑定后重新校验，确保 ActorActionTrack/Clip 立即变为有效状态。
            currentTimeline.Validate();

            float contentLength = GetContentLength(currentTimeline);
            currentTimeline.Play(0f, contentLength, Slate.Cutscene.WrapMode.Once, Finish);
        }

        public void Stop()
        {
            // Stop 可能同步触发 Cutscene 的完成回调并再次进入 Stop，先摘除字段避免重入空引用。
            Cutscene timeline = currentTimeline;
            currentTimeline = null;

            if (timeline != null)
            {
                timeline.Stop(Slate.Cutscene.StopMode.Skip);
                if (timeline != null)
                {
                    Destroy(timeline.gameObject);
                }
            }

            currentContext = null;
            currentCallback = null;
        }

        /// <summary>计算时间轴实际内容长度（最后一个 clip 的结束时间），避免使用 Cutscene.length 默认值导致播放过长。</summary>
        private static float GetContentLength(Cutscene cutscene)
        {
            float maxEnd = 0f;
            if (cutscene == null)
            {
                return maxEnd;
            }

            foreach (var group in cutscene.groups)
            {
                if (group == null)
                {
                    continue;
                }

                foreach (var track in group.tracks)
                {
                    if (track == null)
                    {
                        continue;
                    }

                    foreach (var clip in track.clips)
                    {
                        if (clip == null)
                        {
                            continue;
                        }

                        maxEnd = Mathf.Max(maxEnd, clip.endTime);
                    }
                }
            }

            // 至少 0.1 秒，避免立即结束
            return Mathf.Max(0.1f, maxEnd);
        }

        private static void DisableCameraTracks(Cutscene cutscene)
        {
            if (cutscene == null)
            {
                return;
            }

            var cameraTracks = cutscene.GetComponentsInChildren<CameraTrack>(true);
            foreach (var cameraTrack in cameraTracks)
            {
                if (cameraTrack == null)
                {
                    continue;
                }

                // Destroy 在运行时会延迟到帧末；期间 Validate 会把 CameraTrack 重新加入，
                // 使 Slate 禁用主相机。直接将轨道设为非激活，保证后续校验也不会执行它。
                cameraTrack.isActive = false;
                cameraTrack.gameObject.SetActive(false);
            }

            cutscene.Validate();
        }

        private void Finish()
        {
            System.Action cb = currentCallback;
            Stop();
            cb?.Invoke();
        }

        private void OnDestroy()
        {
            Stop();
        }
    }
}
