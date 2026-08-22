using FMBG.Combat;
using UnityEngine;

namespace FMBG.Timeline
{
    /// <summary>在指定时间点生成投射物（瞬时事件）。</summary>
    public sealed class SpawnProjectileClip : SkillTimelineClip
    {
        [Header("Projectile")]
        public Projectile projectilePrefab;
        public Transform muzzlePoint;

        [Tooltip("未配置枪口节点时，相对施法者生成子弹的位置。")]
        public Vector3 fallbackMuzzleOffset = new(0f, 0.8f, 0.6f);
        public float projectileSpeed = 18f;
        public float projectileLifetime = 3f;
        public float spreadAngle;
        public int projectileCount = 1;
        public LayerMask targetLayers = ~0;

        public override string DefaultName => "Spawn Projectile";
        protected override SkillTrackType DefaultTrackType => SkillTrackType.Gameplay;
        public override string Info => "Spawn Projectile";

        public override void OnEnter(SkillClipContext context)
        {
            if (!context.CanResolve || context.SkillContext == null || context.Combat == null || context.Actor == null)
            {
                Debug.LogWarning("[Skill] 投射物生成失败：施法上下文或 Actor 未绑定。");
                return;
            }

            RangedWeaponConfig rangedConfig = context.SkillContext.Weapon as RangedWeaponConfig;
            Projectile resolvedPrefab = rangedConfig != null && rangedConfig.ProjectilePrefab != null
                ? rangedConfig.ProjectilePrefab
                : projectilePrefab;

            if (resolvedPrefab == null)
            {
                Debug.LogWarning($"[Skill] {context.SkillContext.Skill?.DisplayName} 未配置投射物 Prefab。");
                return;
            }

            Transform actorTransform = context.Actor;
            Vector3 originPosition = muzzlePoint != null
                ? muzzlePoint.position
                : actorTransform.TransformPoint(fallbackMuzzleOffset);
            Vector3 aimDirection = GetAimDirection(context, originPosition);

            float resolvedSpeed = rangedConfig != null ? rangedConfig.ProjectileSpeed : projectileSpeed;
            float resolvedLifetime = rangedConfig != null ? rangedConfig.ProjectileLifetime : projectileLifetime;
            float resolvedSpread = rangedConfig != null ? rangedConfig.SpreadAngle : spreadAngle;
            int resolvedCount = rangedConfig != null ? rangedConfig.ProjectileCount : projectileCount;
            LayerMask resolvedLayers = rangedConfig != null ? rangedConfig.TargetLayers : targetLayers;

            for (int i = 0; i < Mathf.Max(1, resolvedCount); i++)
            {
                Quaternion spread = Quaternion.Euler(
                    0f,
                    Random.Range(-resolvedSpread, resolvedSpread),
                    0f);

                Vector3 direction = spread * aimDirection;
                Projectile projectile = Object.Instantiate(
                    resolvedPrefab,
                    originPosition,
                    Quaternion.LookRotation(direction, Vector3.up));

                projectile.Initialize(
                    direction,
                    resolvedSpeed,
                    resolvedLifetime,
                    context.SkillContext.CalculateDamage(),
                    context.Combat.Faction,
                    context.SkillContext.Caster != null ? context.SkillContext.Caster.gameObject : actorTransform.gameObject,
                    resolvedLayers);
            }
        }

        private static Vector3 GetAimDirection(SkillClipContext context, Vector3 originPosition)
        {
            Vector3 targetPosition = context.SkillContext.Target != null
                ? context.SkillContext.Target.position
                : context.SkillContext.TargetPosition;

            Vector3 dir = targetPosition - originPosition;
            dir.y = 0f;
            return dir.sqrMagnitude > 0.001f ? dir.normalized : context.Actor.forward;
        }
    }
}
