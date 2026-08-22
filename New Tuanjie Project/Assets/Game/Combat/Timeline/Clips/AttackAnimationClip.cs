using FMBG.Visual;
using UnityEngine;

namespace FMBG.Timeline
{
    /// <summary>攻击动画：进入时触发角色攻击动画，时长与动画播放一致。</summary>
    public sealed class AttackAnimationClip : SkillTimelineClip
    {
        public override string DefaultName => "Attack Animation";
        protected override SkillTrackType DefaultTrackType => SkillTrackType.Animation;

        public override void OnEnter(SkillClipContext context)
        {
            if (context == null || context.Actor == null || !context.CanResolve)
            {
                return;
            }

            CharacterVisual visual = context.Actor.GetComponentInChildren<CharacterVisual>(true);
            if (visual != null)
            {
                visual.PlayAttackAnimation(Mathf.Max(0.1f, Duration));
            }
        }
    }
}
