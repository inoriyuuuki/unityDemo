using FMBG.Combat;
using NUnit.Framework;
using UnityEngine;

namespace FMBG.Tests
{
    /// <summary>T9/T10 逻辑验证（EditMode，不依赖真实物理模拟）。</summary>
    public class WeaponLogicTests
    {
        /// <summary>验证近战 OverlapBox 判定能命中范围内目标（T10 命中逻辑）。</summary>
        [Test]
        public void T10_MeleeHitbox_OverlapsTargetInRange()
        {
            // 敌人目标（带 collider + Health）
            var targetGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            targetGo.name = "MeleeTarget";
            targetGo.transform.position = new Vector3(0f, 1f, 1f);
            targetGo.layer = LayerMask.NameToLayer("Enemy");
            var targetHealth = targetGo.AddComponent<Health>();
            targetHealth.Initialize(100f);

            // 用 OverlapBox 模拟 MeleeWeapon 的命中检测
            var cfg = ScriptableObject.CreateInstance<MeleeWeaponConfig>();
            var hitboxField = typeof(MeleeWeaponConfig).GetField("hitboxSize",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            hitboxField.SetValue(cfg, new Vector3(1.2f, 1.5f, 1.8f));
            var offsetField = typeof(MeleeWeaponConfig).GetField("hitboxOffset",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            offsetField.SetValue(cfg, new Vector3(0f, 0.8f, 1f));
            var layersField = typeof(WeaponConfig).GetField("targetLayers",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            layersField.SetValue(cfg, (LayerMask)LayerMask.GetMask("Enemy", "Player"));

            var origin = new Vector3(0f, 1f, 0f);
            Vector3 center = origin + cfg.HitboxOffset;
            Vector3 half = cfg.HitboxSize * 0.5f;

            // 物理同步确保 collider 已注册
            Physics.SyncTransforms();
            var hits = Physics.OverlapBox(center, half, Quaternion.identity, cfg.TargetLayers);
            Assert.IsNotEmpty(hits, "近战命中盒应包含目标");

            // 造成伤害
            var damageable = hits[0].GetComponentInParent<IDamageable>();
            Assert.IsNotNull(damageable);
            damageable.TakeDamage(new DamageInfo(20f, null, null, hits[0].ClosestPoint(center)));
            Assert.AreEqual(80f, targetHealth.CurrentHealth, "应造成一次 20 伤害");

            Object.DestroyImmediate(targetGo);
            Object.DestroyImmediate(cfg);
        }

        /// <summary>验证 DamageInfo 阵营过滤：玩家不能伤害玩家（T2 延伸）。</summary>
        [Test]
        public void T9_FactionFilter_BlocksFriendlyFire()
        {
            var a = new GameObject("A").AddComponent<FactionMember>();
            a.SetFaction(Faction.Player);
            var b = new GameObject("B").AddComponent<FactionMember>();
            b.SetFaction(Faction.Player);

            Assert.IsFalse(a.CanDamage(b), "玩家不应能伤害玩家");
            Object.DestroyImmediate(a.gameObject);
            Object.DestroyImmediate(b.gameObject);
        }
    }
}
