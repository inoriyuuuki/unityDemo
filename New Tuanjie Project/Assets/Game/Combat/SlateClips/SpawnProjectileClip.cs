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
            if (!CanResolve || projectilePrefab == null)
            {
                return;
            }

            var context = GetContext();
            if (context == null || context.Combat == null)
            {
                return;
            }

            Vector3 aimDirection = GetAimDirection(context);
            Transform origin = muzzlePoint != null ? muzzlePoint : actor.transform;

            for (int i = 0; i < Mathf.Max(1, projectileCount); i++)
            {
                Quaternion spread = Quaternion.Euler(
                    0f,
                    Random.Range(-spreadAngle, spreadAngle),
                    0f);

                Vector3 direction = spread * aimDirection;
                Projectile projectile = Instantiate(
                    projectilePrefab,
                    origin.position,
                    Quaternion.LookRotation(direction, Vector3.up));

                projectile.Initialize(
                    direction,
                    projectileSpeed,
                    projectileLifetime,
                    context.CalculateDamage(),
                    context.Combat.Faction,
                    context.Caster != null ? context.Caster.gameObject : actor,
                    targetLayers);
            }
        }

        private Vector3 GetAimDirection(SkillExecutionContext context)
        {
            Vector3 targetPosition = context.Target != null
                ? context.Target.position
                : context.TargetPosition;

            Vector3 dir = targetPosition - actor.transform.position;
            dir.y = 0f;
            return dir.sqrMagnitude > 0.001f ? dir.normalized : actor.transform.forward;
        }
    }
}
