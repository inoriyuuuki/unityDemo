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
    /// <summary>玩家通过 Slate 技能时间轴结算近战伤害、投射物与相机行为。</summary>
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

        [UnityTest]
        public IEnumerator Camera_RemainsActiveAndFollowsPlayerDuringSkill()
        {
            Camera mainCamera = Camera.main;
            Assert.IsNotNull(mainCamera);

            mainCamera.transform.position = player.transform.position + new Vector3(0f, 10f, -5f);
            Assert.IsNotNull(mainCamera.GetComponent<FMBG.Cameras.TopDownCamera>());

            WeaponConfig meleeWeapon = GetPrivateField<WeaponConfig>(input, "meleeWeapon");
            bool cast = skillController.TryCast(
                input.MeleeSkill,
                new SkillCastRequest(target.transform.position, null),
                meleeWeapon);
            Assert.IsTrue(cast);

            yield return new WaitForSeconds(0.1f);
            Assert.IsTrue(mainCamera.gameObject.activeInHierarchy,
                "技能时间轴不应禁用主相机");

            float startX = mainCamera.transform.position.x;
            player.transform.position += Vector3.right * 4f;
            Physics.SyncTransforms();
            yield return new WaitForSeconds(0.6f);

            Assert.Greater(mainCamera.transform.position.x, startX + 2f,
                "攻击期间主相机仍应继续跟随玩家");
        }

        [Test]
        public void PlayerInvincibility_BlocksDamage()
        {
            var debugMode = player.GetComponent<PlayerInvincibilityDebug>();
            Health health = player.GetComponent<Health>();
            Assert.IsNotNull(debugMode, "Player 应挂载 F1 无敌模式组件");
            Assert.IsNotNull(health);

            health.Initialize(100f);
            health.SetInvincible(true);
            health.TakeDamage(new DamageInfo(
                50f,
                target.gameObject,
                target.Combat.Faction,
                player.transform.position));
            Assert.AreEqual(100f, health.CurrentHealth, "无敌模式开启时不应受到伤害");

            health.SetInvincible(false);
            health.TakeDamage(new DamageInfo(
                10f,
                target.gameObject,
                target.Combat.Faction,
                player.transform.position));
            Assert.AreEqual(90f, health.CurrentHealth, "关闭无敌模式后应正常受到伤害");
        }

        [Test]
        public void GameResult_WaitsUntilEveryEnemyIsDead()
        {
            var resultUI = Object.FindObjectOfType<FMBG.UI.GameResultUI>();
            EnemyActor[] enemyActors = Object.FindObjectsOfType<EnemyActor>();
            Assert.IsNotNull(resultUI);
            Assert.GreaterOrEqual(enemyActors.Length, 3, "Main 场景应包含全部战斗敌人");

            FieldInfo finishedField = typeof(FMBG.UI.GameResultUI).GetField(
                "finished",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(finishedField);

            for (int i = 0; i < enemyActors.Length - 1; i++)
            {
                enemyActors[i].Health.TakeDamage(new DamageInfo(
                    9999f,
                    player,
                    player.GetComponent<FactionMember>(),
                    enemyActors[i].transform.position));
            }

            Assert.IsFalse((bool)finishedField.GetValue(resultUI),
                "仍有敌人存活时不能显示胜利");

            EnemyActor lastEnemy = enemyActors[enemyActors.Length - 1];
            lastEnemy.Health.TakeDamage(new DamageInfo(
                9999f,
                player,
                player.GetComponent<FactionMember>(),
                lastEnemy.transform.position));

            Assert.IsTrue((bool)finishedField.GetValue(resultUI),
                "所有敌人死亡后才应显示胜利");
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
