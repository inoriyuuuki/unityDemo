using UnityEngine;

namespace FMBG.SlateClips
{
    /// <summary>冲刺/突进：按目标方向位移。</summary>
    public sealed class DashClip : SkillClipBase
    {
        [SerializeField, HideInInspector] private float _length = 0.25f;
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

        public float dashDistance = 3f;
        public bool useTargetDirection = true;
        public Vector3 fixedDirection = Vector3.forward;
        public AnimationCurve speedCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public override string info => $"Dash {dashDistance}m";

        protected override void OnUpdate(float time)
        {
            var context = GetContext();
            if (context == null || !CanResolve)
            {
                return;
            }

            float length = Mathf.Max(0.01f, this.length);
            float t = Mathf.Clamp01(time / length);
            float delta = speedCurve.Evaluate(t) * dashDistance * (Time.deltaTime / length);

            Vector3 direction;
            if (useTargetDirection)
            {
                Vector3 targetPosition = context.Target != null
                    ? context.Target.position
                    : context.TargetPosition;
                direction = targetPosition - actor.transform.position;
                direction.y = 0f;
                if (direction.sqrMagnitude <= 0.001f)
                {
                    direction = actor.transform.forward;
                }
                else
                {
                    direction.Normalize();
                }
            }
            else
            {
                direction = fixedDirection.normalized;
            }

            actor.transform.position += direction * delta;
        }
    }
}
