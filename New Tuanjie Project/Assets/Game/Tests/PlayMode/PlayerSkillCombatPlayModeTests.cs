using System.Collections;
using System.Reflection;
using FMBG.AI;
using FMBG.Characters;
using FMBG.Combat;
using FMBG.Skills;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace FMBG.Tests
{
    /// <summary>玩家通过 Slate 技能时间轴结算近战伤害并生成远程投射物。</summary>
    public sealed class PlayerSkillCombatPlayModeTests
    {
        private GameObject player;
        private PlayerCombatInput input;
        private CharacterSkillController skillController;
        private EnemyActor target;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            SceneManager.LoadScene("Main");
            yield return null;

            foreach (var runner in Object.FindObjectsOfType<EnemyStateMachineRunner>(true))
            {
                runner.enabled = false;
            }

            foreach (var agent in Object.FindObjectsOfType<UnityEngine.AI.NavMeshAgent>(true))
            {
                agent.enabled = false;
            }

            player = GameObject.Find("Player");
            Assert.IsNotNull(player, "Main 场景应包含 Player");

            input = player.GetComponent<PlayerCombatInput>();
            skillController = player.GetComponent<CharacterSkillController>();
            Assert.IsNotNull(input);
            Assert.IsNotNull(skillController);

            target = Object.FindObjectOfType<EnemyActor>();
            Assert.IsNotNull(target, "Main 场景应至少包含一个敌人");

            player.transform.SetPositionAndRotation(new Vector3(0f, 1f, 0f), Quaternion.identity);
            target.transform.SetPositionAndRotation(new Vector3(0f, 1f, 1.25f), Quaternion.identity);
            target.Health.Initialize(100f);

            Assert.AreEqual(LayerMask.NameToLayer("Enemy"), target.gameObject.layer);
            Assert.AreEqual(Faction.Enemy, target.GetComponent<FactionMember>().Faction);
            Physics.SyncTransforms();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (skillController != null)
            {
                skillController.Interrupt();
            }

            yield return null;

            GameObject directorCameraRoot = GameObject.Find("★ Director Camera Root");
            if (directorCameraRoot != null)
            {
                Object.Destroy(directorCameraRoot);
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator PlayerMeleeSkill_DealsDamageInsideHitWindow()
        {
            WeaponConfig meleeWeapon = GetPrivateField<WeaponConfig>(input, "meleeWeapon");
            Assert.IsNotNull(meleeWeapon);
            Assert.AreEqual(SkillTargetType.Direction, input.MeleeSkill.TargetType);

            bool cast = skillController.TryCast(
                input.MeleeSkill,
                new SkillCastRequest(target.transform.position, null),
                meleeWeapon);

            Assert.IsTrue(cast, "方向型近战技能不应因鼠标目标点距离而拒绝施法");
            yield return new WaitForSeconds(0.25f);

            Assert.Less(target.Health.CurrentHealth, 100f, "近战命中窗口应对敌人造成伤害");
        }

        [UnityTest]
        public IEnumerator PlayerRangedSkill_SpawnsProjectileAndDealsDamage()
        {
            target.transform.position = new Vector3(0f, 1f, 4f);
            target.Health.Initialize(100f);
            Physics.SyncTransforms();

            WeaponConfig rangedWeapon = GetPrivateField<WeaponConfig>(input, "rangedWeapon");
            Assert.IsInstanceOf<RangedWeaponConfig>(rangedWeapon);
            Assert.IsNotNull(((RangedWeaponConfig)rangedWeapon).ProjectilePrefab,
                "手枪配置必须引用投射物 Prefab");

            bool cast = skillController.TryCast(
                input.RangedSkill,
                new SkillCastRequest(target.transform.position, null),
                rangedWeapon);

            Assert.IsTrue(cast);
            yield return new WaitForSeconds(0.1f);

            Assert.IsNotEmpty(Object.FindObjectsOfType<Projectile>(),
                "远程时间轴应在 0.05 秒生成可见投射物");

            yield return new WaitForSeconds(0.3f);
            Assert.Less(target.Health.CurrentHealth, 100f, "投射物命中敌人后应造成伤害");
        }

        private static T GetPrivateField<T>(object owner, string fieldName) where T : class
        {
            FieldInfo field = owner.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            return field != null ? field.GetValue(owner) as T : null;
        }
    }
}
