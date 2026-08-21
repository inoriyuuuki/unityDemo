using FMBG.Visual;
using UnityEngine;

namespace FMBG.SlateClips
{
    /// <summary>技能攻击动画：播放时触发角色攻击动画（手臂挥砍+身体前倾），与伤害窗口同步。</summary>
    public sealed class AttackAnimationClip : SkillClipBase
    {
        [SerializeField, HideInInspector] private float _length = 0.4f;
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

        public override string info => "Attack Animation";

        protected override void OnEnter()
        {
            if (actor == null || !Application.isPlaying)
            {
                return;
            }

            var visual = actor.GetComponentInChildren<CharacterVisual>(true);
            if (visual != null)
            {
                visual.PlayAttackAnimation(Mathf.Max(0.1f, length));
            }
        }
    }
}
