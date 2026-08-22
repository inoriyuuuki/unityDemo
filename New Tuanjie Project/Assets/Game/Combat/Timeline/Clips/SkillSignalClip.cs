using System;
using UnityEngine;

namespace FMBG.Timeline
{
    /// <summary>发送技能事件信号（瞬时事件）。</summary>
    public sealed class SkillSignalClip : SkillTimelineClip
    {
        public string signalName = "OnSkillSignal";

        public static event Action<string, GameObject> OnSignal;

        public override string DefaultName => "Skill Signal";
        protected override SkillTrackType DefaultTrackType => SkillTrackType.Gameplay;
        public override string Info => "Signal: " + signalName;

        public override void OnEnter(SkillClipContext context)
        {
            if (!context.CanResolve || context.Actor == null)
            {
                return;
            }

            OnSignal?.Invoke(signalName, context.Actor.gameObject);
        }
    }
}
