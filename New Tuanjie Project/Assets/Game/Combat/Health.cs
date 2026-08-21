using System;
using UnityEngine;

namespace FMBG.Combat
{
    /// <summary>生命值组件，玩家与敌人共用。</summary>
    public sealed class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 100f;

        public float CurrentHealth { get; private set; }
        public float MaxHealth => maxHealth;
        public bool IsAlive => CurrentHealth > 0f;

        public event Action<DamageInfo> Damaged;
        public event Action<DamageInfo> Died;

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        /// <summary>供 EnemyActor 等按配置初始化。</summary>
        public void Initialize(float maxHealthValue)
        {
            maxHealth = Mathf.Max(1f, maxHealthValue);
            CurrentHealth = maxHealth;
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (!IsAlive)
            {
                return;
            }

            CurrentHealth = Mathf.Max(0f, CurrentHealth - damageInfo.Amount);
            Damaged?.Invoke(damageInfo);

            if (!IsAlive)
            {
                Died?.Invoke(damageInfo);
            }
        }
    }
}
