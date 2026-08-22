using System.Collections.Generic;
using FMBG.Combat;
using UnityEngine;

namespace FMBG.Timeline
{
    /// <summary>近战伤害窗口：持续时间内开启命中检测，过滤重复目标。</summary>
    public sealed class MeleeHitWindowClip : SkillTimelineClip
    {
        [Header("Hitbox")]
        public Vector3 hitboxOffset = new(0f, 0.8f, 1f);
        public Vector3 hitboxSize = new(1.2f, 1.5f, 1.8f);
        public LayerMask targetLayers = ~0;
        public bool detectDuringEntireDuration = true;

        private readonly Collider[] hitResults = new Collider[16];
        private HashSet<IDamageable> damagedTargets;

        public override string DefaultName => "Melee Hit Window";
        protected override SkillTrackType DefaultTrackType => SkillTrackType.Gameplay;

        public override void OnEnter(SkillClipContext context)
        {
            if (!context.CanResolve)
            {
                return;
            }

            damagedTargets = new HashSet<IDamageable>();
            if (context.SkillContext != null && context.SkillContext.HitTargets != null)
            {
                // 复用上下文中的命中记录，避免一次施法多次窗口重复伤害
                damagedTargets = context.SkillContext.HitTargets;
            }
        }

        public override void OnUpdate(SkillClipContext context, float localTime)
        {
            if (!context.CanResolve || damagedTargets == null || context.Actor == null)
            {
                return;
            }

            PerformHitDetection(context);
        }

        public override void OnExit(SkillClipContext context)
        {
            damagedTargets = null;
        }

        public override void OnReverse(SkillClipContext context)
        {
            damagedTargets = null;
        }

        private void PerformHitDetection(SkillClipContext context)
        {
            if (context == null || context.Combat == null || damagedTargets == null || context.Actor == null)
            {
                return;
            }

            Vector3 center = context.Actor.TransformPoint(hitboxOffset);
            Vector3 halfExtents = new(
                hitboxSize.x * 0.5f,
                hitboxSize.y * 0.5f,
                hitboxSize.z * 0.5f);

            int hitCount = Physics.OverlapBoxNonAlloc(
                center,
                halfExtents,
                hitResults,
                context.Actor.rotation,
                targetLayers,
                QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hitCount; i++)
            {
                if (!hitResults[i].TryGetComponentInParent(out IDamageable target))
                {
                    continue;
                }

                if (!IsValidTarget(context, target))
                {
                    continue;
                }

                if (!damagedTargets.Add(target))
                {
                    continue;
                }

                Vector3 hitPoint = hitResults[i].ClosestPoint(center);
                target.TakeDamage(new DamageInfo(
                    context.SkillContext.CalculateDamage(),
                    context.SkillContext.Caster != null ? context.SkillContext.Caster.gameObject : context.Actor.gameObject,
                    context.Combat.Faction,
                    hitPoint,
                    context.Actor.forward,
                    0f,
                    DamageType.Melee));
            }
        }

        private static bool IsValidTarget(SkillClipContext context, IDamageable target)
        {
            if (target is not Component targetComponent)
            {
                return false;
            }

            if (targetComponent.gameObject == context.Combat.gameObject)
            {
                return false;
            }

            if (context.Combat.Faction == null ||
                !targetComponent.TryGetComponentInParent(out FactionMember targetFaction))
            {
                return true;
            }

            return context.Combat.Faction.CanDamage(targetFaction);
        }
    }
}
