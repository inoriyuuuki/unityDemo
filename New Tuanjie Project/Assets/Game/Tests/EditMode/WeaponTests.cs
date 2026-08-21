using FMBG.Combat;
using NUnit.Framework;
using UnityEngine;

namespace FMBG.Tests
{
    /// <summary>T3 攻击范围判定 + T4 近战去重。</summary>
    public class WeaponTests
    {
        private MeleeWeaponConfig CreateMeleeConfig(float range)
        {
            var cfg = ScriptableObject.CreateInstance<MeleeWeaponConfig>();
            var f = typeof(MeleeWeaponConfig).GetField("attackRange",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            f.SetValue(cfg, range);
            return cfg;
        }

        /// <summary>测试辅助：直接挂武器到 combat（绕过 prefab 实例化）。</summary>
        private void EquipTestWeapon(CharacterCombat combat, WeaponConfig cfg)
        {
            var weaponGo = new GameObject("Weapon");
            var weapon = weaponGo.AddComponent<MeleeWeapon>();
            weapon.Initialize(combat, cfg);
            var f = typeof(CharacterCombat).GetField("<CurrentWeapon>k__BackingField",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (f == null)
            {
                // 兜底：遍历查找包含 CurrentWeapon 的字段
                foreach (var fi in typeof(CharacterCombat).GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic))
                {
                    if (fi.Name.Contains("CurrentWeapon"))
                    {
                        f = fi;
                        break;
                    }
                }
            }
            f.SetValue(combat, weapon);
        }

        [Test]
        public void T3_IsTargetInAttackRange_Melee()
        {
            var ownerGo = new GameObject("Owner");
            var combat = ownerGo.AddComponent<CharacterCombat>();
            var cfg = CreateMeleeConfig(1.8f);

            // 通过测试辅助设置武器
            EquipTestWeapon(combat, cfg);
            Assert.IsNotNull(combat.CurrentWeapon, "装备后应有武器");

            var targetGo = new GameObject("Target");
            targetGo.transform.position = new Vector3(1f, 0f, 0f);

            Assert.IsTrue(combat.IsTargetInAttackRange(targetGo.transform));

            var farGo = new GameObject("Far");
            farGo.transform.position = new Vector3(3f, 0f, 0f);
            Assert.IsFalse(combat.IsTargetInAttackRange(farGo.transform));

            Object.DestroyImmediate(ownerGo);
            Object.DestroyImmediate(targetGo);
            Object.DestroyImmediate(farGo);
            Object.DestroyImmediate(cfg);
        }

        [Test]
        public void T3_IsTargetInAttackRange_WithTolerance()
        {
            var ownerGo = new GameObject("Owner");
            var combat = ownerGo.AddComponent<CharacterCombat>();
            var cfg = CreateMeleeConfig(1.8f);
            EquipTestWeapon(combat, cfg);

            var targetGo = new GameObject("Target");
            targetGo.transform.position = new Vector3(1.9f, 0f, 0f);

            // 1.9 > 1.8，但 tolerance 0.4 → 在滞后区间内
            Assert.IsTrue(combat.IsTargetInAttackRange(targetGo.transform, 0.4f));
            Assert.IsFalse(combat.IsTargetInAttackRange(targetGo.transform, 0f));

            Object.DestroyImmediate(ownerGo);
            Object.DestroyImmediate(targetGo);
            Object.DestroyImmediate(cfg);
        }
    }
}
