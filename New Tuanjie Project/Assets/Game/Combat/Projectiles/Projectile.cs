using UnityEngine;

namespace FMBG.Combat
{
    /// <summary>实体弹丸：沿方向飞行，命中 IDamageable 造成伤害，命中障碍销毁。</summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 15f;
        [SerializeField] private float lifetime = 3f;

        private Vector3 direction;
        private float damage;
        private FactionMember sourceFaction;
        private GameObject source;
        private LayerMask hitLayers;
        private bool initialized;

        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.isKinematic = false;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }
        }

        public void Initialize(
            Vector3 fireDirection,
            float projectileSpeed,
            float projectileLifetime,
            float damageAmount,
            FactionMember faction,
            GameObject sourceObject,
            LayerMask targetLayers)
        {
            direction = fireDirection.normalized;
            speed = projectileSpeed;
            lifetime = projectileLifetime;
            damage = damageAmount;
            sourceFaction = faction;
            source = sourceObject;
            hitLayers = targetLayers;
            initialized = true;

            if (rb != null)
            {
                rb.velocity = direction * speed;
            }
        }

        private void Start()
        {
            if (!initialized)
            {
                Destroy(gameObject);
                return;
            }

            Destroy(gameObject, lifetime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!initialized)
            {
                return;
            }

            // 只检测目标层
            if ((hitLayers & (1 << other.gameObject.layer)) == 0)
            {
                return;
            }

            if (other.TryGetComponentInParent(out IDamageable damageable))
            {
                if (damageable is Component damageableComponent)
                {
                    if (damageableComponent.gameObject == source)
                    {
                        return;
                    }

                    if (damageableComponent.TryGetComponentInParent(out FactionMember targetFaction))
                    {
                        if (sourceFaction != null && !sourceFaction.CanDamage(targetFaction))
                        {
                            return;
                        }
                    }
                }

                Vector3 hitPoint = other.ClosestPoint(transform.position);
                damageable.TakeDamage(new DamageInfo(
                    damage,
                    source,
                    sourceFaction,
                    hitPoint,
                    direction,
                    0f,
                    DamageType.Projectile));
            }

            Destroy(gameObject);
        }
    }
}
