using UnityEngine;

namespace FMBG.SlateClips
{
    /// <summary>面向技能目标（持续时间）。</summary>
    public sealed class FaceTargetClip : SkillClipBase
    {
        [SerializeField, HideInInspector] private float _length = 0.2f;
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

        public override string info => "Face Target";

        protected override void OnUpdate(float time)
        {
            var context = GetContext();
            if (context == null || context.Caster == null)
            {
                return;
            }

            Vector3 targetPosition = context.Target != null
                ? context.Target.position
                : context.TargetPosition;

            var combat = context.Combat;
            if (combat != null)
            {
                combat.FaceTowards(targetPosition);
            }
        }
    }
}
