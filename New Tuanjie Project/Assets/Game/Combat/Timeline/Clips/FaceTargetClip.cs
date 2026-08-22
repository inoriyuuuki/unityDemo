using UnityEngine;

namespace FMBG.Timeline
{
    /// <summary>面向技能目标（持续时间内每帧朝向目标点/锁定单位）。</summary>
    public sealed class FaceTargetClip : SkillTimelineClip
    {
        public override string DefaultName => "Face Target";
        protected override SkillTrackType DefaultTrackType => SkillTrackType.Animation;

        public override void OnUpdate(SkillClipContext context, float localTime)
        {
            if (context == null || context.SkillContext == null || context.SkillContext.Caster == null)
            {
                return;
            }

            Vector3 targetPosition = context.SkillContext.Target != null
                ? context.SkillContext.Target.position
                : context.SkillContext.TargetPosition;

            if (context.Combat != null)
            {
                context.Combat.FaceTowards(targetPosition);
            }
        }
    }
}
