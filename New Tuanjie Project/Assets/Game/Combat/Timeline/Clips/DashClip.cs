using UnityEngine;

namespace FMBG.Timeline
{
    /// <summary>冲刺/突进：按目标方向（或固定方向）位移，速度由曲线控制。</summary>
    public sealed class DashClip : SkillTimelineClip
    {
        public float dashDistance = 3f;
        public bool useTargetDirection = true;
        public Vector3 fixedDirection = Vector3.forward;
        public AnimationCurve speedCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public override string DefaultName => "Dash";
        protected override SkillTrackType DefaultTrackType => SkillTrackType.Gameplay;
        public override string Info => $"Dash {dashDistance}m";

        public override void OnUpdate(SkillClipContext context, float localTime)
        {
            if (context == null || context.Actor == null || !context.CanResolve || context.SkillContext == null)
            {
                return;
            }

            float length = Mathf.Max(0.01f, Duration);
            float t = Mathf.Clamp01(localTime / length);
            float delta = speedCurve.Evaluate(t) * dashDistance * (Time.deltaTime / length);

            Vector3 direction;
            if (useTargetDirection)
            {
                Vector3 targetPosition = context.SkillContext.Target != null
                    ? context.SkillContext.Target.position
                    : context.SkillContext.TargetPosition;
                direction = targetPosition - context.Actor.position;
                direction.y = 0f;
                if (direction.sqrMagnitude <= 0.001f)
                {
                    direction = context.Actor.forward;
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

            context.Actor.position += direction * delta;
        }
    }
}
