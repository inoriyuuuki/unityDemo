using UnityEngine;

namespace FMBG.Combat
{
    public enum DamageType
    {
        Melee,
        Projectile,
        Explosion
    }

    /// <summary>统一伤害信息，玩家与敌人共用。</summary>
    public readonly struct DamageInfo
    {
        public readonly float Amount;
        public readonly GameObject Source;
        public readonly FactionMember SourceFaction;
        public readonly Vector3 HitPoint;
        public readonly Vector3 HitDirection;
        public readonly float Knockback;
        public readonly DamageType DamageType;

        public DamageInfo(
            float amount,
            GameObject source,
            FactionMember sourceFaction,
            Vector3 hitPoint,
            Vector3 hitDirection = default,
            float knockback = 0f,
            DamageType damageType = DamageType.Melee)
        {
            Amount = amount;
            Source = source;
            SourceFaction = sourceFaction;
            HitPoint = hitPoint;
            HitDirection = hitDirection;
            Knockback = knockback;
            DamageType = damageType;
        }
    }
}
