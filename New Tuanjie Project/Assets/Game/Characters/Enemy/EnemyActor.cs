using FMBG.Combat;
using FMBG.Skills;
using UnityEngine;

namespace FMBG.AI
{
    /// <summary>敌人实体：按固定依赖顺序初始化各组件。</summary>
    public sealed class EnemyActor : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private EnemyConfig config;

        [Header("Components")]
        [SerializeField] private Health health;
        [SerializeField] private EnemyMotor motor;
        [SerializeField] private EnemyPerception perception;
        [SerializeField] private CharacterCombat combat;
        [SerializeField] private CharacterSkillController skillController;
        [SerializeField] private SkillSelector skillSelector;
        [SerializeField] private EnemyStateMachineRunner stateMachine;
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private Transform returnPoint;

        public EnemyConfig Config => config;
        public Health Health => health;
        public EnemyMotor Motor => motor;
        public EnemyPerception Perception => perception;
        public CharacterCombat Combat => combat;
        public CharacterSkillController SkillController => skillController;
        public SkillSelector SkillSelector => skillSelector;
        public EnemyStateMachineRunner StateMachine => stateMachine;

        private void Awake()
        {
            // 场景序列化已有配置才自动初始化；运行时 AddComponent 的（如测试）由调用方显式 Initialize
            if (config != null)
            {
                Initialize(config);
            }
        }

        public void Initialize(EnemyConfig enemyConfig)
        {
            if (enemyConfig == null)
            {
                Debug.LogError("EnemyConfig为空。", this);
                enabled = false;
                return;
            }

            config = enemyConfig;

            if (health != null)
            {
                health.Initialize(config.Vitals.maxHealth);
            }

            if (motor != null)
            {
                motor.Initialize(config.Movement);
            }

            if (perception != null)
            {
                perception.Initialize(config.Perception);
            }

            if (combat != null && config.Combat.startingWeapon != null)
            {
                combat.Equip(config.Combat.startingWeapon);
            }

            if (stateMachine != null)
            {
                stateMachine.Initialize(this, config);
            }
        }

        public Vector3[] GetPatrolPoints()
        {
            if (patrolPoints == null || patrolPoints.Length == 0)
            {
                return System.Array.Empty<Vector3>();
            }

            var result = new Vector3[patrolPoints.Length];
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                result[i] = patrolPoints[i] != null ? patrolPoints[i].position : transform.position;
            }

            return result;
        }

        public Vector3 GetReturnPosition()
        {
            if (returnPoint != null)
            {
                return returnPoint.position;
            }

            if (patrolPoints != null && patrolPoints.Length > 0 && patrolPoints[0] != null)
            {
                return patrolPoints[0].position;
            }

            return transform.position;
        }
    }
}
