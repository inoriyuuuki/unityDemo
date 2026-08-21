using FMBG.AI;
using NUnit.Framework;
using UnityEngine;

namespace FMBG.Tests
{
    /// <summary>T5 敌人配置 OnValidate 钳制。</summary>
    public class ConfigTests
    {
        [Test]
        public void T5_EnemyConfig_OnValidate_ClampsValues()
        {
            var cfg = ScriptableObject.CreateInstance<EnemyConfig>();
            var field = typeof(EnemyConfig).GetField("vitals",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            // maxHealth 0 → 钳制为 1
            field.SetValue(cfg, new EnemyVitalsSettings { maxHealth = 0f, hitStunDuration = 0f, deathDestroyDelay = 0f });
            cfg.OnValidatePublic();
            Assert.GreaterOrEqual(cfg.Vitals.maxHealth, 1f);

            // viewAngle 500 → 钳制为 360
            var perceptionField = typeof(EnemyConfig).GetField("perception",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            perceptionField.SetValue(cfg, new EnemyPerceptionSettings
            {
                viewDistance = 10f,
                viewAngle = 500f,
                scanInterval = 0.1f,
                alertDuration = 0.6f,
                forgetDuration = 3f
            });
            cfg.OnValidatePublic();
            Assert.LessOrEqual(cfg.Perception.viewAngle, 360f);

            Object.DestroyImmediate(cfg);
        }
    }

    public static class EnemyConfigTestExtensions
    {
        /// <summary>通过反射调用私有 OnValidate。</summary>
        public static void OnValidatePublic(this EnemyConfig cfg)
        {
            var m = typeof(EnemyConfig).GetMethod("OnValidate",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            m?.Invoke(cfg, null);
        }
    }
}
