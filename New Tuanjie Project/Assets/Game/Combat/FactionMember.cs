using UnityEngine;

namespace FMBG.Combat
{
    public enum Faction
    {
        Player,
        Enemy,
        Neutral
    }

    /// <summary>阵营成员，决定谁可以伤害谁。</summary>
    public sealed class FactionMember : MonoBehaviour
    {
        [field: SerializeField]
        public Faction Faction { get; private set; }

        public bool CanDamage(FactionMember other)
        {
            if (other == null)
            {
                return true;
            }

            return Faction switch
            {
                Faction.Player => other.Faction == Faction.Enemy,
                Faction.Enemy => other.Faction == Faction.Player,
                _ => false
            };
        }
    }
}
