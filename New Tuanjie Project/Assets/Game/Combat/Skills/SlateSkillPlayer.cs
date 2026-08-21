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

            // 绑定施法者为 ActorGroup 的 actor
            if (context.Caster != null)
            {
                var groups = currentTimeline.GetComponentsInChildren<ActorGroup>(true);
                foreach (var group in groups)
                {
                    group.actor = context.Caster.gameObject;
                }
            }

            currentTimeline.Play(0f, currentTimeline.length, Slate.Cutscene.WrapMode.Once, Finish);
        }

        public void Stop()
        {
            if (currentTimeline != null)
            {
                currentTimeline.Stop(Slate.Cutscene.StopMode.Skip);
                Destroy(currentTimeline.gameObject);
                currentTimeline = null;
            }

            currentContext = null;
            currentCallback = null;
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
