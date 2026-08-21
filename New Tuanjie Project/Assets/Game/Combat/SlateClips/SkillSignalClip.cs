using System;
using UnityEngine;

namespace FMBG.SlateClips
{
    /// <summary>发送技能事件信号（瞬时事件）。</summary>
    public sealed class SkillSignalClip : SkillClipBase
    {
        public string signalName = "OnSkillSignal";

        public static event Action<string, GameObject> OnSignal;

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

        public override string info => "Signal: " + signalName;

        protected override void OnEnter()
        {
            if (!CanResolve || actor == null)
            {
                return;
            }

            OnSignal?.Invoke(signalName, actor.gameObject);
        }
    }
}
