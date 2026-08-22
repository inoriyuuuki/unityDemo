using UnityEngine;

namespace FMBG.Timeline
{
    /// <summary>创建技能特效（瞬时事件）。</summary>
    public sealed class SpawnEffectClip : SkillTimelineClip
    {
        public GameObject effectPrefab;
        public Vector3 offset = Vector3.zero;
        public float destroyDelay = 2f;

        public override string DefaultName => "Spawn Effect";
        protected override SkillTrackType DefaultTrackType => SkillTrackType.Effect;
        public override string Info => effectPrefab != null ? "Spawn Effect: " + effectPrefab.name : "Spawn Effect";

        public override void OnEnter(SkillClipContext context)
        {
            if (!context.CanResolve || effectPrefab == null || context.Actor == null)
            {
                return;
            }

            GameObject effect = Object.Instantiate(
                effectPrefab,
                context.Actor.position + context.Actor.TransformVector(offset),
                context.Actor.rotation);

            if (destroyDelay > 0f)
            {
                Object.Destroy(effect, destroyDelay);
            }
        }
    }
}
