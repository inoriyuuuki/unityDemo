using UnityEngine;

namespace FMBG.Combat
{
    [CreateAssetMenu(
        fileName = "RangedWeaponConfig",
        menuName = "Game/Combat/Ranged Weapon Config")]
    public sealed class RangedWeaponConfig : WeaponConfig
    {
        [Header("Range")]
        [SerializeField, Min(0f)] private float minAttackRange;
        [SerializeField, Min(0.1f)] private float maxAttackRange = 10f;
        [SerializeField, Min(0.1f)] private float preferredAttackRange = 7f;

        [Header("Projectile")]
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField, Min(0.1f)] private float projectileSpeed = 15f;
        [SerializeField, Min(0f)] private float projectileLifetime = 3f;
        [SerializeField, Range(0f, 45f)] private float spreadAngle;
        [SerializeField, Min(1)] private int projectileCount = 1;

        [Header("Effects")]
        [SerializeField] private GameObject muzzleFlashPrefab;
        [SerializeField] private AudioClip fireSound;

        public override float MinAttackRange => minAttackRange;
        public override float MaxAttackRange => maxAttackRange;
        public override float PreferredAttackRange => preferredAttackRange;

        public Projectile ProjectilePrefab => projectilePrefab;
        public float ProjectileSpeed => projectileSpeed;
        public float ProjectileLifetime => projectileLifetime;
        public float SpreadAngle => spreadAngle;
        public int ProjectileCount => projectileCount;

        public GameObject MuzzleFlashPrefab => muzzleFlashPrefab;
        public AudioClip FireSound => fireSound;

        private void OnValidate()
        {
            minAttackRange = Mathf.Max(0f, minAttackRange);
            maxAttackRange = Mathf.Max(minAttackRange + 0.1f, maxAttackRange);
            preferredAttackRange = Mathf.Clamp(preferredAttackRange, minAttackRange, maxAttackRange);
        }
    }
}
