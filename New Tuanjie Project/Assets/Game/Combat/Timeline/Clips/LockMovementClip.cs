using UnityEngine;

namespace FMBG.Timeline
{
    /// <summary>锁定/解锁角色移动。</summary>
    public sealed class LockMovementClip : SkillTimelineClip
    {
        public bool lockMovement = true;

        public override string DefaultName => "Lock Movement";
        protected override SkillTrackType DefaultTrackType => SkillTrackType.Gameplay;
        public override string Info => lockMovement ? "Lock Movement" : "Unlock Movement";

        public override void OnEnter(SkillClipContext context)
        {
            if (context != null && context.Caster != null)
            {
                context.Caster.SetMovementLock(lockMovement);
            }
        }

        public override void OnReverse(SkillClipContext context)
        {
            if (context != null && context.Caster != null)
            {
                context.Caster.SetMovementLock(!lockMovement);
            }
        }
    }
}
