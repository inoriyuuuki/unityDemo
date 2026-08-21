using System.Collections.Generic;
using FMBG.Skills;
using UnityEngine;

namespace FMBG.Combat
{
    /// <summary>武器静态配置基类（只读模板）。</summary>
    public abstract class WeaponConfig : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string weaponId;
        [SerializeField] private string displayName;

        [Header("Runtime")]
        [SerializeField] private Weapon weaponPrefab;

        [Header("Damage")]
        [SerializeField, Min(0f)] private float damage = 10f;
        [SerializeField, Min(0f)] private float knockbackForce;

        [Header("Attack Timing")]
        [SerializeField, Min(0f)] private float windupDuration = 0.2f;
        [SerializeField, Min(0f)] private float activeDuration = 0.1f;
        [SerializeField, Min(0f)] private float recoveryDuration = 0.4f;

        [Header("Movement")]
        [SerializeField] private bool canMoveWhileAttacking = true;
        [SerializeField, Range(0f, 1f)] private float attackMoveSpeedMultiplier = 0.5f;

        [Header("Target")]
        [SerializeField] private LayerMask targetLayers;

        public string WeaponId => weaponId;
        public string DisplayName => displayName;
        public Weapon WeaponPrefab => weaponPrefab;

        public float Damage => damage;
        public float KnockbackForce => knockbackForce;

        public float WindupDuration => windupDuration;
        public float ActiveDuration => activeDuration;
        public float RecoveryDuration => recoveryDuration;

        public bool CanMoveWhileAttacking => canMoveWhileAttacking;
        public float AttackMoveSpeedMultiplier => attackMoveSpeedMultiplier;
        public LayerMask TargetLayers => targetLayers;

        [Header("Skills")]
        [SerializeField] private SkillConfig basicAttack;
        [SerializeField] private SkillConfig[] skills;

        public SkillConfig BasicAttack => basicAttack;
        public IReadOnlyList<SkillConfig> Skills => skills;

        public abstract float MinAttackRange { get; }
        public abstract float MaxAttackRange { get; }
        public abstract float PreferredAttackRange { get; }
    }
}
