using System.Collections.Generic;
using FMBG.Combat;
using FMBG.Skills;
using UnityEngine;

namespace FMBG.SlateClips
{
    /// <summary>近战伤害窗口：持续时间内开启命中检测，过滤重复目标。</summary>
    public sealed class MeleeHitWindowClip : SkillClipBase
    {
        [Header("Duration")]
        [SerializeField, HideInInspector] private float _length = 0.15f;
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

        [Header("Hitbox")]
        public Vector3 hitboxOffset = new(0f, 0.8f, 1f);
        public Vector3 hitboxSize = new(1.2f, 1.5f, 1.8f);
        public LayerMask targetLayers = ~0;
        public bool detectDuringEntireDuration = true;

        private readonly Collider[] hitResults = new Collider[16];
        private HashSet<IDamageable> damagedTargets;

        public override string info => "Melee Hit Window";

        protected override void OnEnter()
        {
            if (!CanResolve)
            {
                return;
            }

            damagedTargets = new HashSet<IDamageable>();

            var context = GetContext();
            if (context != null && context.HitTargets != null)
            {
                // 复用上下文中的命中记录，避免一次施法多次窗口重复伤害
                damagedTargets = context.HitTargets;
            }
        }

        protected override void OnUpdate(float time)
        {
            if (!CanResolve || damagedTargets == null)
            {
                return;
            }

            PerformHitDetection();
        }

        protected override void OnExit()
        {
        }

        protected override void OnReverse()
        {
        }

        private void PerformHitDetection()
        {
            var context = GetContext();
            if (context == null || context.Combat == null || damagedTargets == null || actor == null)
            {
                return;
            }

            Vector3 center = actor.transform.TransformPoint(hitboxOffset);
            Vector3 halfExtents = new(
                hitboxSize.x * 0.5f,
                hitboxSize.y * 0.5f,
                hitboxSize.z * 0.5f);

            int hitCount = Physics.OverlapBoxNonAlloc(
                center,
                halfExtents,
                hitResults,
                actor.transform.rotation,
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
                    context.CalculateDamage(),
                    context.Caster != null ? context.Caster.gameObject : actor,
                    context.Combat.Faction,
                    hitPoint,
                    actor.transform.forward,
                    0f,
                    DamageType.Melee));
            }
        }

        private static bool IsValidTarget(SkillExecutionContext context, IDamageable target)
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
