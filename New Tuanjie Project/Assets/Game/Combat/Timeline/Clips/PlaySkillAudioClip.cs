using UnityEngine;

namespace FMBG.Timeline
{
    /// <summary>播放技能音效（瞬时事件）。</summary>
    public sealed class PlaySkillAudioClip : SkillTimelineClip
    {
        public AudioClip audioClip;
        [Range(0f, 1f)] public float volume = 1f;

        public override string DefaultName => "Play Audio";
        protected override SkillTrackType DefaultTrackType => SkillTrackType.Audio;
        public override string Info => audioClip != null ? "Play Audio: " + audioClip.name : "Play Audio";

        public override void OnEnter(SkillClipContext context)
        {
            if (!context.CanResolve || audioClip == null || context.Actor == null)
            {
                return;
            }

            AudioSource source = context.Actor.GetComponent<AudioSource>();
            if (source == null)
            {
                source = context.Actor.gameObject.AddComponent<AudioSource>();
            }

            source.PlayOneShot(audioClip, volume);
        }
    }
}
