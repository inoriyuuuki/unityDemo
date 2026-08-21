using UnityEngine;

namespace FMBG.Combat
{
    [CreateAssetMenu(
        fileName = "MeleeWeaponConfig",
        menuName = "Game/Combat/Melee Weapon Config")]
    public sealed class MeleeWeaponConfig : WeaponConfig
    {
        [Header("Melee Range")]
        [SerializeField, Min(0.1f)] private float attackRange = 1.8f;
        [SerializeField, Min(0.1f)] private float preferredAttackRange = 1.4f;

        [Header("Hitbox")]
        [SerializeField] private Vector3 hitboxOffset = new(0f, 0.8f, 1f);
        [SerializeField] private Vector3 hitboxSize = new(1.2f, 1.5f, 1.8f);
        [SerializeField] private bool detectDuringEntireActivePhase = true;
        [SerializeField] private bool canHitMultipleTargets = true;

        [Header("Effects")]
        [SerializeField] private GameObject swingEffectPrefab;
        [SerializeField] private AudioClip swingSound;
        [SerializeField] private AudioClip hitSound;

        public override float MinAttackRange => 0f;
        public override float MaxAttackRange => attackRange;
        public override float PreferredAttackRange => preferredAttackRange;

        public Vector3 HitboxOffset => hitboxOffset;
        public Vector3 HitboxSize => hitboxSize;
        public bool DetectDuringEntireActivePhase => detectDuringEntireActivePhase;
        public bool CanHitMultipleTargets => canHitMultipleTargets;

        public GameObject SwingEffectPrefab => swingEffectPrefab;
        public AudioClip SwingSound => swingSound;
        public AudioClip HitSound => hitSound;

        private void OnValidate()
        {
            attackRange = Mathf.Max(0.1f, attackRange);
            preferredAttackRange = Mathf.Clamp(preferredAttackRange, 0.1f, attackRange);

            hitboxSize.x = Mathf.Max(0.1f, hitboxSize.x);
            hitboxSize.y = Mathf.Max(0.1f, hitboxSize.y);
            hitboxSize.z = Mathf.Max(0.1f, hitboxSize.z);
        }
    }
}
