using UnityEngine;

namespace FMBG.SlateClips
{
    /// <summary>播放技能音效（瞬时事件）。</summary>
    public sealed class PlaySkillAudioClip : SkillClipBase
    {
        public AudioClip audioClip;
        [Range(0f, 1f)] public float volume = 1f;

        [SerializeField, HideInInspector] private float _length = 0.01f;
        [SerializeField, HideInInspector] private float _blendIn = 0f;
        [SerializeField, HideInInspector] private float _blendOut = 0f;

        public override float length
        {
            get { return _length; }
            set { _length = Mathf.Max(0f, value); }
        }

        public override float blendIn
        {
            get { return _blendIn; }
            set { _blendIn = value; }
        }

        public override float blendOut
        {
            get { return _blendOut; }
            set { _blendOut = value; }
        }

        public override string info => audioClip != null ? "Play Audio: " + audioClip.name : "Play Audio";

        protected override void OnEnter()
        {
            if (!CanResolve || audioClip == null || actor == null)
            {
                return;
            }

            var source = actor.GetComponent<AudioSource>();
            if (source == null)
            {
                source = actor.gameObject.AddComponent<AudioSource>();
            }

            source.PlayOneShot(audioClip, volume);
        }
    }
}
