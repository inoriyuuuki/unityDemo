using System.Collections;
using FMBG.AI;
using FMBG.Combat;
using FMBG.Skills;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEditor;

namespace FMBG.Tests
{
    /// <summary>T7-T11 PlayMode 集成测试。</summary>
    public class EnemyAiPlayModeTests
    {
        /// <summary>
        /// PlayMode 测试会话默认是空场景：没有烘焙 NavMesh，运行时新增的 NavMeshAgent
        /// 无法绑定（"no valid NavMesh"），导致 Motor 移动失效、状态机卡在 Return。
        /// 显式加载 Main 场景以提供 NavMesh（与 PlayerSkillCombatPlayModeTests 一致）。
        /// </summary>
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            SceneManager.LoadScene("Main");
            yield return null;
        }

        private EnemyConfig LoadConfig()
        {
            // 使用项目中已配置的默认敌人（真实状态图 + 感知 + 行为参数）
            return AssetDatabase.LoadAssetAtPath<EnemyConfig>(
                "Assets/Game/Configs/Enemies/Enemy_MeleeGrunt.asset");
        }

        /// <summary>搭建一个带完整 AI 组件的敌人。</summary>
        private (GameObject enemy, EnemyActor actor, EnemyPerception perception, EnemyStateMachineRunner runner) BuildEnemy(Vector3 pos)
        {
            var go = new GameObject("Enemy_" + Random.value);
            go.transform.position = pos;
            var health = go.AddComponent<Health>();
            var motor = go.AddComponent<EnemyMotor>();
            var perception = go.AddComponent<EnemyPerception>();
            var combat = go.AddComponent<CharacterCombat>();
            var runner = go.AddComponent<EnemyStateMachineRunner>();
            var actor = go.AddComponent<EnemyActor>();

            var t = typeof(EnemyActor);
            SetField(actor, t, "health", health);
            SetField(actor, t, "motor", motor);
            SetField(actor, t, "perception", perception);
            SetField(actor, t, "combat", combat);
            SetField(actor, t, "stateMachine", runner);

            return (go, actor, perception, runner);
        }

        /// <summary>
        /// 隔离 Main 场景中已存在的玩家（位于原点附近，会被视野 10m 感知），
        /// 避免测试敌人误锁定场景玩家导致"移动测试玩家后仍可见"的误判。
        /// 必须在创建测试玩家之前调用（二者同名 "Player"）。
        /// </summary>
        private static void MoveScenePlayerAway()
        {
            var scenePlayer = GameObject.Find("Player");
            if (scenePlayer != null)
            {
                scenePlayer.transform.position = new Vector3(1000f, -200f, 1000f);
                Physics.SyncTransforms();
            }
        }

        [UnityTest]
        public IEnumerator T7_Enemy_SeesPlayer_TransitionsToChase()
        {
            MoveScenePlayerAway();
            var (go, actor, perception, runner) = BuildEnemy(Vector3.zero);
            actor.Initialize(LoadConfig());

            var playerGo = new GameObject("Player");
            playerGo.transform.position = new Vector3(0f, 1f, 5f);
            playerGo.layer = LayerMask.NameToLayer("Player");
            var playerCol = playerGo.AddComponent<BoxCollider>();
            playerCol.size = Vector3.one;

            yield return new WaitForSeconds(3f);

            Assert.IsTrue(perception.CanSeeTarget, "敌人应能看到玩家");
            Assert.IsTrue(runner.CurrentState is ChaseStateNode or AttackStateNode,
                "敌人应进入 Chase/Attack，实际: " + (runner.CurrentState?.GetType().Name ?? "null"));

            Object.Destroy(go);
            Object.Destroy(playerGo);
        }

        [UnityTest]
        public IEnumerator T8_Enemy_LosesTarget_Investigates_ThenReturns()
        {
            MoveScenePlayerAway();
            var (go, actor, perception, runner) = BuildEnemy(Vector3.zero);
            actor.Initialize(LoadConfig());

            var playerGo = new GameObject("Player");
            playerGo.transform.position = new Vector3(0f, 1f, 5f);
            playerGo.layer = LayerMask.NameToLayer("Player");
            var playerCol = playerGo.AddComponent<BoxCollider>();
            playerCol.size = Vector3.one;

            // 让敌人发现玩家
            yield return new WaitForSeconds(3f);
            Assert.IsTrue(perception.CanSeeTarget, "敌人应能看到玩家");

            // 玩家瞬移到远处（超出视野与感知半径）
            playerGo.transform.position = new Vector3(100f, 1f, 100f);
            yield return new WaitForSeconds(5f);

            // 警戒值应下降，敌人应丢失目标
            Assert.IsFalse(perception.CanSeeTarget, "玩家离开后敌人应丢失目标");

            // 最终应回到巡逻/闲置
            yield return new WaitForSeconds(6f);
            Assert.IsTrue(runner.CurrentState is PatrolStateNode or IdleStateNode,
                "敌人应回到 Patrol/Idle，实际: " + (runner.CurrentState?.GetType().Name ?? "null"));

            Object.Destroy(go);
            Object.Destroy(playerGo);
        }

        [UnityTest]
        public IEnumerator T11_TwoEnemies_ShareGraph_IndependentState()
        {
            MoveScenePlayerAway();
            var config = LoadConfig();

            // 两个敌人共享同一张图（通过同一 config），玩家放在两者前方。
            // 注意：视野 10m 且墙体遮挡（Wall_1 在 x=2），敌人2放在 (0,0,3)：
            //   距玩家 (0,1,5) 约 2m、视线沿 x=0 无墙体遮挡、且在 NavMesh 上。
            var (e1, a1, p1, r1) = BuildEnemy(new Vector3(0f, 0f, 0f));
            var (e2, a2, p2, r2) = BuildEnemy(new Vector3(0f, 0f, 3f));

            a1.Initialize(config);
            a2.Initialize(config);

            var playerGo = new GameObject("Player");
            playerGo.transform.position = new Vector3(0f, 1f, 5f);
            playerGo.layer = LayerMask.NameToLayer("Player");
            var playerCol = playerGo.AddComponent<BoxCollider>();
            playerCol.size = Vector3.one;

            yield return new WaitForSeconds(3f);

            // 两个敌人应该都看到玩家并进入攻击/追击
            Assert.IsTrue(p1.CanSeeTarget, "敌人1应看到玩家");
            Assert.IsTrue(p2.CanSeeTarget, "敌人2应看到玩家");

            // 黑板数据独立：手动改变一个敌人的黑板不影响另一个
            var ctx1 = r1.Context;
            var ctx2 = r2.Context;
            ctx1.Blackboard.PatrolPointIndex = 2;
            Assert.AreNotEqual(ctx1.Blackboard.PatrolPointIndex, ctx2.Blackboard.PatrolPointIndex,
                "黑板数据应相互独立");

            Object.Destroy(e1);
            Object.Destroy(e2);
            Object.Destroy(playerGo);
        }

        private static void SetField(object obj, System.Type type, string name, object value)
        {
            var f = type.GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            f?.SetValue(obj, value);
        }
    }
}
