using FMBG.Combat;
using NUnit.Framework;
using UnityEngine;

namespace FMBG.Tests
{
    /// <summary>T1 伤害系统 + T2 阵营系统。</summary>
    public class CombatTests
    {
        [Test]
        public void T1_Health_TakeDamage_ReducesHealth()
        {
            var go = new GameObject("Health");
            var health = go.AddComponent<Health>();
            health.Initialize(100f);

            health.TakeDamage(new DamageInfo(30f, null, null, Vector3.zero));

            Assert.AreEqual(70f, health.CurrentHealth);
            Assert.IsTrue(health.IsAlive);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void T1_Health_Death_FiresDiedEvent_Once()
        {
            var go = new GameObject("Health");
            var health = go.AddComponent<Health>();
            health.Initialize(10f);
            int diedCount = 0;
            health.Died += _ => diedCount++;

            health.TakeDamage(new DamageInfo(10f, null, null, Vector3.zero));
            health.TakeDamage(new DamageInfo(5f, null, null, Vector3.zero));

            Assert.IsFalse(health.IsAlive);
            Assert.AreEqual(0f, health.CurrentHealth);
            Assert.AreEqual(1, diedCount);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void T1_Health_OverDamage_ClampsToZero()
        {
            var go = new GameObject("Health");
            var health = go.AddComponent<Health>();
            health.Initialize(20f);
            health.TakeDamage(new DamageInfo(50f, null, null, Vector3.zero));
            Assert.AreEqual(0f, health.CurrentHealth);
            Assert.IsFalse(health.IsAlive);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void T2_Enemy_CanDamage_OnlyPlayer()
        {
            var enemyGo = new GameObject("Enemy");
            var enemy = enemyGo.AddComponent<FactionMember>();
            enemy.SetFaction(Faction.Enemy);

            var playerGo = new GameObject("Player");
            var player = playerGo.AddComponent<FactionMember>();
            player.SetFaction(Faction.Player);

            var neutralGo = new GameObject("Neutral");
            var neutral = neutralGo.AddComponent<FactionMember>();
            neutral.SetFaction(Faction.Neutral);

            Assert.IsTrue(enemy.CanDamage(player));
            Assert.IsFalse(enemy.CanDamage(enemy));
            Assert.IsFalse(enemy.CanDamage(neutral));

            Object.DestroyImmediate(enemyGo);
            Object.DestroyImmediate(playerGo);
            Object.DestroyImmediate(neutralGo);
        }
    }
}
