using FMBG.Combat;
using FMBG.Skills;
using UnityEngine;

namespace FMBG.SlateClips
{
    /// <summary>在指定时间点生成投射物（瞬时事件）。</summary>
    public sealed class SpawnProjectileClip : SkillClipBase
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

        public override string info => "Spawn Projectile";

        protected override void OnEnter()
        {
            if (!CanResolve)
            {
                return;
            }

            var context = GetContext();
            if (context == null || context.Combat == null || actor == null)
            {
                Debug.LogWarning("[Skill] 投射物生成失败：施法上下文或 Actor 未绑定。", this);
                return;
            }

            var rangedConfig = context.Weapon as RangedWeaponConfig;
            Projectile resolvedPrefab = rangedConfig != null && rangedConfig.ProjectilePrefab != null
                ? rangedConfig.ProjectilePrefab
                : projectilePrefab;

            if (resolvedPrefab == null)
            {
                Debug.LogWarning($"[Skill] {context.Skill?.DisplayName} 未配置投射物 Prefab。", this);
                return;
            }

            Transform actorTransform = actor.transform;
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
                Projectile projectile = Instantiate(
                    resolvedPrefab,
                    originPosition,
                    Quaternion.LookRotation(direction, Vector3.up));

                projectile.Initialize(
                    direction,
                    resolvedSpeed,
                    resolvedLifetime,
                    context.CalculateDamage(),
                    context.Combat.Faction,
                    context.Caster != null ? context.Caster.gameObject : actor,
                    resolvedLayers);
            }
        }

        private Vector3 GetAimDirection(SkillExecutionContext context, Vector3 originPosition)
        {
            Vector3 targetPosition = context.Target != null
                ? context.Target.position
                : context.TargetPosition;

            Vector3 dir = targetPosition - originPosition;
            dir.y = 0f;
            return dir.sqrMagnitude > 0.001f ? dir.normalized : actor.transform.forward;
        }
    }
}
