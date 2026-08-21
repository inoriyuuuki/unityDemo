using UnityEngine;

namespace FMBG.SlateClips
{
    /// <summary>锁定/解锁角色移动。</summary>
    public sealed class LockMovementClip : SkillClipBase
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

        public bool lockMovement = true;

        public override string info => lockMovement ? "Lock Movement" : "Unlock Movement";

        protected override void OnEnter()
        {
            var context = GetContext();
            if (context != null && context.Caster != null)
            {
                context.Caster.SetMovementLock(lockMovement);
            }
        }

        protected override void OnReverse()
        {
            var context = GetContext();
            if (context != null && context.Caster != null)
            {
                context.Caster.SetMovementLock(!lockMovement);
            }
        }
    }
}
