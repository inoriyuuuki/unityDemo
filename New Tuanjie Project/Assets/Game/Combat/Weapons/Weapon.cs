using UnityEngine;

namespace FMBG.Combat
{
    /// <summary>武器运行时基类。只保存攻击过程中的动态数据，不写回配置。</summary>
    public abstract class Weapon : MonoBehaviour
    {
        public CharacterCombat Owner { get; private set; }
        public WeaponConfig Config { get; private set; }

        public bool IsAttacking { get; protected set; }

        public float MinAttackRange => Config != null ? Config.MinAttackRange : 0f;
        public float MaxAttackRange => Config != null ? Config.MaxAttackRange : 0f;
        public float PreferredAttackRange => Config != null ? Config.PreferredAttackRange : 0f;

        public void Initialize(CharacterCombat owner, WeaponConfig config)
        {
            Owner = owner;
            Config = config;

            ValidateConfig(config);
            OnInitialized();
        }

        protected virtual void OnInitialized()
        {
        }

        protected abstract void ValidateConfig(WeaponConfig config);

        public abstract bool CanAttack();

        public abstract bool TryAttack(WeaponAttackContext attackContext);

        public abstract void CancelAttack();

        protected DamageInfo CreateDamageInfo(Vector3 hitPoint)
        {
            return new DamageInfo(
                Config != null ? Config.Damage : 0f,
                Owner != null ? Owner.gameObject : gameObject,
                Owner != null ? Owner.Faction : null,
                hitPoint,
                default,
                Config != null ? Config.KnockbackForce : 0f,
                DamageType.Melee);
        }
    }
}
