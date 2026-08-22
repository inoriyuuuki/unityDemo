using System.Collections.Generic;
using FMBG.AI;
using FMBG.Combat;
using FMBG.Skills;
using FMBG.Timeline;
using UnityEditor;
using UnityEngine;

namespace FMBG.EditorTools
{
    /// <summary>一键生成演示所需的配置资产与 GraphView 状态图。</summary>
    public static class GameAssetFactory
    {
        [MenuItem("Game/Tools/Create Demo Assets")]
        public static void CreateDemoAssets()
        {
            EnsureFolder("Assets/Game/Configs/Weapons");
            EnsureFolder("Assets/Game/Configs/Enemies");
            EnsureFolder("Assets/Game/Configs/Graphs");

            CreateSword();
            CreatePistol();
            CreateStateGraph();
            CreateMeleeGrunt();
            CreateRangedGuard();
            CreateSwordSlashSkill();
            CreatePistolShotSkill();
            WireSwordSkills();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[GameAssetFactory] 演示资产创建完成。");
        }

        /// <summary>创建条件节点并连接：statePort -> conditionNode -> targetState。</summary>
        private static EnemyTransitionData AddTransition(
            EnemyStateGraph graph,
            EnemyStateNodeData from,
            EnemyStateNodeData to,
            TransitionCondition condition,
            Vector2 position)
        {
            if (graph == null || from == null || to == null)
            {
                return null;
            }

            return graph.AddTransition(from.Id, to.Id, condition, position);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = path.Substring(0, path.LastIndexOf('/'));
            string leaf = path.Substring(path.LastIndexOf('/') + 1);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        private static MeleeWeaponConfig CreateSword()
        {
            var asset = LoadOrCreate<MeleeWeaponConfig>("Assets/Game/Configs/Weapons/Weapon_Sword.asset");
            if (asset == null)
            {
                return null;
            }

            asset.name = "Weapon_Sword";
            asset.SetDisplayName("长剑");
            asset.SetDamage(20f);
            asset.SetTiming(0.15f, 0.15f, 0.35f);
            asset.SetRange(1.8f, 1.4f);
            asset.SetHitbox(new Vector3(0f, 0.8f, 1f), new Vector3(1.2f, 1.5f, 1.8f));
            asset.SetTargetLayers(LayerMask.GetMask("Enemy", "Player"));
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static RangedWeaponConfig CreatePistol()
        {
            var asset = LoadOrCreate<RangedWeaponConfig>("Assets/Game/Configs/Weapons/Weapon_Pistol.asset");
            if (asset == null)
            {
                return null;
            }

            asset.name = "Weapon_Pistol";
            asset.SetDisplayName("手枪");
            asset.SetDamage(12f);
            asset.SetTiming(0.1f, 0.1f, 0.3f);
            asset.SetRanges(0f, 9f, 6f);
            asset.SetProjectile(
                AssetDatabase.LoadAssetAtPath<FMBG.Combat.Projectile>("Assets/Game/Prefabs/Projectile_Pistol.prefab"),
                18f, 2.5f, 0f, 1);
            asset.SetTargetLayers(LayerMask.GetMask("Enemy", "Player"));
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static EnemyStateGraph CreateStateGraph()
        {
            var graph = LoadOrCreate<EnemyStateGraph>("Assets/Game/Configs/Graphs/Enemy_DefaultGraph.asset");
            if (graph == null)
            {
                return null;
            }

            graph.Clear();

            graph.EntryNodePosition = new Vector2(-600f, 0f);
            graph.AnyStateNodePosition = new Vector2(300f, 300f);

            var idle = graph.AddState(EnemyStateType.Idle, new Vector2(-300f, -100f));
            var patrol = graph.AddState(EnemyStateType.Patrol, new Vector2(-300f, 100f));
            var chase = graph.AddState(EnemyStateType.Chase, new Vector2(0f, 0f));
            var attack = graph.AddState(EnemyStateType.Attack, new Vector2(300f, -100f));
            var investigate = graph.AddState(EnemyStateType.Investigate, new Vector2(300f, 100f));
            var returnNode = graph.AddState(EnemyStateType.Return, new Vector2(600f, 150f));
            var dead = graph.AddState(EnemyStateType.Dead, new Vector2(600f, -250f));

            graph.SetEntry(idle.Id);
            graph.SetDeathTarget(dead.Id);

            // Idle
            var idleToPatrol = new TransitionCondition(TransitionConditionType.TimerElapsed);
            idleToPatrol.SetDuration(2f);
            AddTransition(graph, idle, patrol, idleToPatrol, new Vector2(-150f, -80f));
            AddTransition(graph, idle, chase, new TransitionCondition(TransitionConditionType.TargetVisible), new Vector2(-150f, -20f));

            // Patrol
            AddTransition(graph, patrol, idle, new TransitionCondition(TransitionConditionType.ReachedDestination), new Vector2(-150f, 140f));
            AddTransition(graph, patrol, chase, new TransitionCondition(TransitionConditionType.TargetVisible), new Vector2(-150f, 200f));

            // Chase
            var inRange = new TransitionCondition(TransitionConditionType.TargetInAttackRange);
            inRange.SetTolerance(0f);
            AddTransition(graph, chase, attack, inRange, new Vector2(150f, 20f));
            AddTransition(graph, chase, investigate, new TransitionCondition(TransitionConditionType.TargetLost), new Vector2(150f, 80f));

            // Attack
            var outOfRange = new TransitionCondition(TransitionConditionType.TargetOutOfAttackRange);
            outOfRange.SetTolerance(0.4f);
            AddTransition(graph, attack, chase, outOfRange, new Vector2(450f, -80f));
            AddTransition(graph, attack, investigate, new TransitionCondition(TransitionConditionType.TargetLost), new Vector2(450f, -20f));

            // Investigate
            AddTransition(graph, investigate, chase, new TransitionCondition(TransitionConditionType.TargetVisible), new Vector2(450f, 140f));
            var investigateTimer = new TransitionCondition(TransitionConditionType.TimerElapsed);
            investigateTimer.SetDuration(3f);
            AddTransition(graph, investigate, returnNode, investigateTimer, new Vector2(450f, 200f));

            // Return
            AddTransition(graph, returnNode, patrol, new TransitionCondition(TransitionConditionType.ReachedDestination), new Vector2(750f, 180f));
            AddTransition(graph, returnNode, chase, new TransitionCondition(TransitionConditionType.TargetVisible), new Vector2(750f, 240f));

            graph.name = "Enemy_DefaultGraph";
            EditorUtility.SetDirty(graph);
            return graph;
        }

        private static EnemyConfig CreateMeleeGrunt()
        {
            var graph = AssetDatabase.LoadAssetAtPath<EnemyStateGraph>(
                "Assets/Game/Configs/Graphs/Enemy_DefaultGraph.asset");
            var weapon = AssetDatabase.LoadAssetAtPath<MeleeWeaponConfig>(
                "Assets/Game/Configs/Weapons/Weapon_Sword.asset");

            var asset = LoadOrCreate<EnemyConfig>("Assets/Game/Configs/Enemies/Enemy_MeleeGrunt.asset");
            if (asset == null)
            {
                return null;
            }

            asset.name = "Enemy_MeleeGrunt";
            asset.SetIdentity("melee_grunt", "近战杂兵");
            asset.SetStateGraph(graph);
            asset.SetVitals(80f, 0f, 2f);
            asset.SetMovement(2f, 4.5f, 12f, 360f, 0.2f);
            asset.SetPerception(10f, 100f, 0.1f, 1.2f, 3f,
                LayerMask.GetMask("Player"), LayerMask.GetMask("Obstacle"));
            asset.SetBehaviour(1f, 3f, 0.15f, 0.4f, 3f, 180f, 0.2f);
            asset.SetCombat(weapon, 360f, 0.3f, false);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static EnemyConfig CreateRangedGuard()
        {
            var graph = AssetDatabase.LoadAssetAtPath<EnemyStateGraph>(
                "Assets/Game/Configs/Graphs/Enemy_DefaultGraph.asset");
            var weapon = AssetDatabase.LoadAssetAtPath<RangedWeaponConfig>(
                "Assets/Game/Configs/Weapons/Weapon_Pistol.asset");

            var asset = LoadOrCreate<EnemyConfig>("Assets/Game/Configs/Enemies/Enemy_RangedGuard.asset");
            if (asset == null)
            {
                return null;
            }

            asset.name = "Enemy_RangedGuard";
            asset.SetIdentity("ranged_guard", "远程守卫");
            asset.SetStateGraph(graph);
            asset.SetVitals(60f, 0f, 2f);
            asset.SetMovement(1.8f, 3.5f, 10f, 360f, 0.2f);
            asset.SetPerception(14f, 120f, 0.1f, 1.2f, 3f,
                LayerMask.GetMask("Player"), LayerMask.GetMask("Obstacle"));
            asset.SetBehaviour(1f, 3f, 0.15f, 0.4f, 3f, 180f, 0.2f);
            asset.SetCombat(weapon, 360f, 0.5f, false);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static void CreateSwordSlashSkill()
        {
            EnsureFolder("Assets/Game/Configs/Skills");
            var skill = LoadOrCreate<SkillConfig>("Assets/Game/Configs/Skills/Skill_SwordSlash.asset");
            if (skill == null)
            {
                return;
            }

            skill.name = "Skill_SwordSlash";
            SetField(skill, "skillId", "sword_slash");
            SetField(skill, "displayName", "挥剑斩");
            SetField(skill, "targetType", SkillTargetType.Direction);
            SetField(skill, "cooldown", 1.2f);
            SetField(skill, "minCastRange", 0f);
            SetField(skill, "maxCastRange", 1.8f);
            SetField(skill, "lockMovement", true);
            SetField(skill, "faceTarget", true);
            SetField(skill, "canBeInterrupted", true);
            SetField(skill, "damageMultiplier", 1f);

            // 创建技能时间轴资源（先配置 Clip，再保存）
            var timeline = CreateSkillTimeline(
                "Assets/Game/Configs/Timelines/Skill_SwordSlash_Timeline.asset",
                timeline =>
                {
                    // Animation Track: 攻击动画 (0.00 - 0.40)
                    timeline.AddClip(new AttackAnimationClip { StartTime = 0f, Duration = 0.4f });

                    // Gameplay Track: 伤害窗口 (0.08 - 0.18)
                    timeline.AddClip(new MeleeHitWindowClip
                    {
                        StartTime = 0.08f,
                        Duration = 0.10f,
                        hitboxOffset = new Vector3(0f, 0.8f, 1f),
                        hitboxSize = new Vector3(1.2f, 1.5f, 1.8f),
                        targetLayers = LayerMask.GetMask("Enemy", "Player")
                    });

                    // Effect Track: 特效 + 音效
                    timeline.AddClip(new SpawnEffectClip
                    {
                        StartTime = 0.06f,
                        Duration = 0.01f,
                        destroyDelay = 0.3f
                    });

                    timeline.AddClip(new PlaySkillAudioClip { StartTime = 0.06f, Duration = 0.01f });
                });

            SetField(skill, "timeline", timeline);
            EditorUtility.SetDirty(skill);
            AssetDatabase.SaveAssets();
        }

        private static void CreatePistolShotSkill()
        {
            EnsureFolder("Assets/Game/Configs/Skills");
            var skill = LoadOrCreate<SkillConfig>("Assets/Game/Configs/Skills/Skill_PistolShot.asset");
            if (skill == null)
            {
                return;
            }

            skill.name = "Skill_PistolShot";
            SetField(skill, "skillId", "pistol_shot");
            SetField(skill, "displayName", "手枪射击");
            SetField(skill, "targetType", SkillTargetType.Position);
            SetField(skill, "cooldown", 0.8f);
            SetField(skill, "minCastRange", 0f);
            SetField(skill, "maxCastRange", 12f);
            SetField(skill, "lockMovement", true);
            SetField(skill, "faceTarget", true);
            SetField(skill, "canBeInterrupted", true);
            SetField(skill, "damageMultiplier", 1f);

            var timeline = CreateSkillTimeline(
                "Assets/Game/Configs/Timelines/Skill_PistolShot_Timeline.asset",
                timeline =>
                {
                    // Animation Track: 攻击动画（后坐力）
                    timeline.AddClip(new AttackAnimationClip { StartTime = 0f, Duration = 0.25f });
                    // Gameplay Track: 生成弹丸 (0.05)
                    timeline.AddClip(new SpawnProjectileClip
                    {
                        StartTime = 0.05f,
                        Duration = 0.01f,
                        projectilePrefab = AssetDatabase.LoadAssetAtPath<FMBG.Combat.Projectile>("Assets/Game/Prefabs/Projectile_Pistol.prefab"),
                        projectileSpeed = 18f,
                        projectileLifetime = 2.5f,
                        spreadAngle = 0f,
                        projectileCount = 1,
                        targetLayers = LayerMask.GetMask("Enemy", "Player")
                    });

                    timeline.AddClip(new PlaySkillAudioClip { StartTime = 0.05f, Duration = 0.01f });
                });

            SetField(skill, "timeline", timeline);
            EditorUtility.SetDirty(skill);
            AssetDatabase.SaveAssets();
        }

        private static void WireSwordSkills()
        {
            var sword = AssetDatabase.LoadAssetAtPath<MeleeWeaponConfig>("Assets/Game/Configs/Weapons/Weapon_Sword.asset");
            var slash = AssetDatabase.LoadAssetAtPath<SkillConfig>("Assets/Game/Configs/Skills/Skill_SwordSlash.asset");
            if (sword != null && slash != null)
            {
                SetField(sword, "basicAttack", slash);
                EditorUtility.SetDirty(sword);
            }

            var pistol = AssetDatabase.LoadAssetAtPath<RangedWeaponConfig>("Assets/Game/Configs/Weapons/Weapon_Pistol.asset");
            var shot = AssetDatabase.LoadAssetAtPath<SkillConfig>("Assets/Game/Configs/Skills/Skill_PistolShot.asset");
            if (pistol != null && shot != null)
            {
                SetField(pistol, "basicAttack", shot);
                EditorUtility.SetDirty(pistol);
            }

            AssetDatabase.SaveAssets();
        }

        private static SkillTimeline CreateSkillTimeline(
            string assetPath,
            System.Action<SkillTimeline> configure)
        {
            string folder = assetPath.Substring(0, assetPath.LastIndexOf('/'));
            EnsureFolder(folder);

            var timeline = LoadOrCreate<SkillTimeline>(assetPath);
            if (timeline == null)
            {
                return null;
            }

            timeline.name = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            timeline.ClearClips();
            timeline.Length = 0.1f;
            configure?.Invoke(timeline);
            timeline.EnsureLengthCoversContent();
            EditorUtility.SetDirty(timeline);
            return timeline;
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
            {
                return existing;
            }

            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        // ---- 便捷设置方法（通过反射访问私有字段，避免额外编辑器 API）----
        private static void SetField(Object obj, string fieldName, object value)
        {
            System.Type type = obj.GetType();
            var flags = System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Public;

            var field = type.GetField(fieldName, flags);
            while (field == null && type.BaseType != null)
            {
                type = type.BaseType;
                field = type.GetField(fieldName, flags);
            }

            if (field != null)
            {
                if (field.FieldType == typeof(LayerMask) && value is int intValue)
                {
                    field.SetValue(obj, (LayerMask)intValue);
                }
                else
                {
                    field.SetValue(obj, value);
                }
            }
            else
            {
                Debug.LogWarning("[GameAssetFactory] 找不到字段: " + fieldName);
            }
        }

        private static void SetDisplayName(this WeaponConfig cfg, string name) => SetField(cfg, "displayName", name);
        private static void SetDamage(this WeaponConfig cfg, float dmg) => SetField(cfg, "damage", dmg);
        private static void SetTiming(this WeaponConfig cfg, float windup, float active, float recovery)
        {
            SetField(cfg, "windupDuration", windup);
            SetField(cfg, "activeDuration", active);
            SetField(cfg, "recoveryDuration", recovery);
        }
        private static void SetRange(this MeleeWeaponConfig cfg, float range, float preferred)
        {
            SetField(cfg, "attackRange", range);
            SetField(cfg, "preferredAttackRange", preferred);
        }
        private static void SetHitbox(this MeleeWeaponConfig cfg, Vector3 offset, Vector3 size)
        {
            SetField(cfg, "hitboxOffset", offset);
            SetField(cfg, "hitboxSize", size);
        }
        private static void SetRanges(this RangedWeaponConfig cfg, float min, float max, float preferred)
        {
            SetField(cfg, "minAttackRange", min);
            SetField(cfg, "maxAttackRange", max);
            SetField(cfg, "preferredAttackRange", preferred);
        }
        private static void SetProjectile(this RangedWeaponConfig cfg, Projectile prefab, float speed, float lifetime, float spread, int count)
        {
            SetField(cfg, "projectilePrefab", prefab);
            SetField(cfg, "projectileSpeed", speed);
            SetField(cfg, "projectileLifetime", lifetime);
            SetField(cfg, "spreadAngle", spread);
            SetField(cfg, "projectileCount", count);
        }
        private static void SetTargetLayers(this WeaponConfig cfg, LayerMask mask) => SetField(cfg, "targetLayers", mask);

        private static void SetIdentity(this EnemyConfig cfg, string id, string name) { SetField(cfg, "enemyId", id); SetField(cfg, "displayName", name); }
        private static void SetStateGraph(this EnemyConfig cfg, EnemyStateGraph g) => SetField(cfg, "stateGraph", g);
        private static void SetVitals(this EnemyConfig cfg, float hp, float stun, float delay) => SetField(cfg, "vitals", new EnemyVitalsSettings { maxHealth = hp, hitStunDuration = stun, deathDestroyDelay = delay });
        private static void SetMovement(this EnemyConfig cfg, float patrol, float chase, float accel, float angular, float tol) => SetField(cfg, "movement", new EnemyMovementSettings { patrolSpeed = patrol, chaseSpeed = chase, acceleration = accel, angularSpeed = angular, stoppingDistanceTolerance = tol });
        private static void SetPerception(this EnemyConfig cfg, float dist, float angle, float interval, float alert, float forget, LayerMask targets, LayerMask obstacles) => SetField(cfg, "perception", new EnemyPerceptionSettings { viewDistance = dist, viewAngle = angle, scanInterval = interval, alertDuration = alert, forgetDuration = forget, targetLayers = targets, obstacleLayers = obstacles });
        private static void SetBehaviour(this EnemyConfig cfg, float minIdle, float maxIdle, float repath, float exitTol, float investigate, float turnSpeed, float returnStop) => SetField(cfg, "behaviour", new EnemyBehaviourSettings { minIdleDuration = minIdle, maxIdleDuration = maxIdle, repathInterval = repath, attackExitRangeTolerance = exitTol, investigateDuration = investigate, investigateTurnSpeed = turnSpeed, returnStoppingDistance = returnStop });
        private static void SetCombat(this EnemyConfig cfg, WeaponConfig weapon, float aimTurn, float reaction, bool los) => SetField(cfg, "combat", new EnemyCombatSettings { startingWeapon = weapon, aimTurnSpeed = aimTurn, reactionDuration = reaction, requireLineOfSightToAttack = los });
    }
}
