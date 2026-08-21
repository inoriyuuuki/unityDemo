using System.Collections.Generic;
using FMBG.AI;
using FMBG.Combat;
using FMBG.Skills;
using FMBG.SlateClips;
using Slate;
using UnityEditor;
using UnityEngine;
using XNode;

namespace FMBG.EditorTools
{
    /// <summary>一键生成演示所需的配置资产与 xNode 状态图。</summary>
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
            CreateSwordSlashSkill();
            CreatePistolShotSkill();
            WireSwordSkills();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[GameAssetFactory] 演示资产创建完成。");
        }

        private static Node CreateNode(EnemyStateGraph graph, System.Type type)
        {
            Node node = graph.AddNode(type);
            if (node == null)
            {
                return null;
            }

            node.name = type.Name;
            if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(graph)))
            {
                AssetDatabase.AddObjectToAsset(node, graph);
            }

            return node;
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
            asset.SetProjectile(null, 18f, 2.5f, 0f, 1);
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

            // 清空旧节点（用 xNode 的 RemoveNode，避免残留已销毁引用）
            while (graph.nodes.Count > 0)
            {
                Node node = graph.nodes[graph.nodes.Count - 1];
                graph.RemoveNode(node);
                Object.DestroyImmediate(node, true);
            }
            AssetDatabase.SaveAssets();

            // 创建节点（与 NodeGraphEditor.CreateNode 一致：AddNode + AddObjectToAsset）
            var entry = CreateNode(graph, typeof(EnemyEntryNode));
            var any = CreateNode(graph, typeof(EnemyAnyStateNode));
            var idle = CreateNode(graph, typeof(IdleStateNode));
            var patrol = CreateNode(graph, typeof(PatrolStateNode));
            var chase = CreateNode(graph, typeof(ChaseStateNode));
            var attack = CreateNode(graph, typeof(AttackStateNode));
            var investigate = CreateNode(graph, typeof(InvestigateStateNode));
            var returnNode = CreateNode(graph, typeof(ReturnStateNode));
            var dead = CreateNode(graph, typeof(DeadStateNode));

            entry.position = new Vector2(-600, 0);
            any.position = new Vector2(300, 300);
            idle.position = new Vector2(-300, -100);
            patrol.position = new Vector2(-300, 100);
            chase.position = new Vector2(0, 0);
            attack.position = new Vector2(300, -100);
            investigate.position = new Vector2(300, 100);
            returnNode.position = new Vector2(600, 150);
            dead.position = new Vector2(600, -250);

            // 连接
            entry.GetOutputPort("start").Connect(idle.GetInputPort("entry"));
            any.GetOutputPort("dead").Connect(dead.GetInputPort("entry"));

            idle.GetOutputPort("patrol").Connect(patrol.GetInputPort("entry"));
            idle.GetOutputPort("chase").Connect(chase.GetInputPort("entry"));

            patrol.GetOutputPort("idle").Connect(idle.GetInputPort("entry"));
            patrol.GetOutputPort("chase").Connect(chase.GetInputPort("entry"));

            chase.GetOutputPort("attack").Connect(attack.GetInputPort("entry"));
            chase.GetOutputPort("investigate").Connect(investigate.GetInputPort("entry"));

            attack.GetOutputPort("chase").Connect(chase.GetInputPort("entry"));
            attack.GetOutputPort("investigate").Connect(investigate.GetInputPort("entry"));

            investigate.GetOutputPort("chase").Connect(chase.GetInputPort("entry"));
            investigate.GetOutputPort("returnNode").Connect(returnNode.GetInputPort("entry"));

            returnNode.GetOutputPort("patrol").Connect(patrol.GetInputPort("entry"));
            returnNode.GetOutputPort("chase").Connect(chase.GetInputPort("entry"));

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
            asset.SetPerception(10f, 100f, 0.1f, 0.6f, 3f,
                LayerMask.GetMask("Player"), LayerMask.GetMask("Obstacle"));
            asset.SetBehaviour(1f, 3f, 0.15f, 0.4f, 3f, 180f, 0.2f);
            asset.SetCombat(weapon, 360f, 0.3f, false);
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
            SetField(skill, "targetType", SkillTargetType.Unit);
            SetField(skill, "cooldown", 1.2f);
            SetField(skill, "minCastRange", 0f);
            SetField(skill, "maxCastRange", 2.5f);
            SetField(skill, "lockMovement", true);
            SetField(skill, "faceTarget", true);
            SetField(skill, "canBeInterrupted", true);
            SetField(skill, "damageMultiplier", 1f);

            // 创建 Slate 时间轴 prefab（先添加 clip，再保存）
            var timeline = CreateSkillTimeline(
                "Assets/Game/Prefabs/Skill_SwordSlash.prefab",
                (casterGroup, gameplayTrack, effectTrack) =>
                {
                    // Gameplay Track: 伤害窗口 (0.20 - 0.35)
                    var hitWindow = gameplayTrack.AddAction<MeleeHitWindowClip>(0.20f);
                    SetField(hitWindow, "hitboxOffset", new Vector3(0f, 0.8f, 1f));
                    SetField(hitWindow, "hitboxSize", new Vector3(1.2f, 1.5f, 1.8f));
                    SetField(hitWindow, "targetLayers", LayerMask.GetMask("Enemy", "Player"));
                    SetProperty(hitWindow, "length", 0.15f);

                    // Effect Track: 特效 + 音效
                    var trail = effectTrack.AddAction<SpawnEffectClip>(0.18f);
                    SetField(trail, "offset", Vector3.zero);
                    SetField(trail, "destroyDelay", 0.3f);

                    effectTrack.AddAction<PlaySkillAudioClip>(0.18f);
                });

            SetField(skill, "timelinePrefab", timeline);
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
                "Assets/Game/Prefabs/Skill_PistolShot.prefab",
                (casterGroup, gameplayTrack, effectTrack) =>
                {
                    // Gameplay Track: 生成弹丸 (0.12)
                    var spawn = gameplayTrack.AddAction<SpawnProjectileClip>(0.12f);
                    SetField(spawn, "projectilePrefab",
                        AssetDatabase.LoadAssetAtPath<FMBG.Combat.Projectile>("Assets/Game/Prefabs/Projectile_Pistol.prefab"));
                    SetField(spawn, "projectileSpeed", 18f);
                    SetField(spawn, "projectileLifetime", 2.5f);
                    SetField(spawn, "spreadAngle", 0f);
                    SetField(spawn, "projectileCount", 1);
                    SetField(spawn, "targetLayers", LayerMask.GetMask("Enemy", "Player"));

                    effectTrack.AddAction<PlaySkillAudioClip>(0.12f);
                });

            SetField(skill, "timelinePrefab", timeline);
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

        private static Cutscene CreateSkillTimeline(
            string prefabPath,
            System.Action<ActorGroup, ActorActionTrack, ActorActionTrack> configure)
        {
            var go = new GameObject("SkillTimeline");
            var cutscene = go.AddComponent<Cutscene>();

            var casterGroup = cutscene.AddGroup<ActorGroup>();
            casterGroup.name = "Caster";

            var gameplayTrack = casterGroup.AddTrack<ActorActionTrack>("Gameplay");
            var effectTrack = casterGroup.AddTrack<ActorActionTrack>("Effect");

            // 保存前先配置 clip（此时对象尚未销毁）
            configure?.Invoke(casterGroup, gameplayTrack, effectTrack);

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
            return prefab != null ? prefab.GetComponent<Cutscene>() : null;
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

        private static void SetProperty(Object obj, string propertyName, object value)
        {
            var prop = obj.GetType().GetProperty(propertyName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.FlattenHierarchy);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(obj, value);
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
