namespace FMBG.Combat
{
    /// <summary>可受伤接口，玩家与敌人共用。</summary>
    public interface IDamageable
    {
        bool IsAlive { get; }

        void TakeDamage(DamageInfo damageInfo);
    }
}
