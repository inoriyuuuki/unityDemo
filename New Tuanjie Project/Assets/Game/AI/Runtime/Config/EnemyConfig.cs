using System;
using FMBG.Combat;
using UnityEngine;

namespace FMBG.AI
{
    [Serializable]
    public struct EnemyVitalsSettings
    {
        [Min(1f)] public float maxHealth;
        [Min(0f)] public float hitStunDuration;
        [Min(0f)] public float deathDestroyDelay;
    }

    [Serializable]
    public struct EnemyMovementSettings
    {
        [Min(0f)] public float patrolSpeed;
        [Min(0f)] public float chaseSpeed;
        [Min(0f)] public float acceleration;
        [Min(0f)] public float angularSpeed;
        [Min(0f)] public float stoppingDistanceTolerance;
    }

    [Serializable]
    public struct EnemyPerceptionSettings
    {
        [Min(0.1f)] public float viewDistance;
        [Range(0f, 360f)] public float viewAngle;
        [Min(0.02f)] public float scanInterval;
        [Min(0f)] public float alertDuration;
        [Min(0f)] public float forgetDuration;
        public LayerMask targetLayers;
        public LayerMask obstacleLayers;
    }

    [Serializable]
    public struct EnemyBehaviourSettings
    {
        [Header("Idle")]
        [Min(0f)] public float minIdleDuration;
        [Min(0f)] public float maxIdleDuration;

        [Header("Chase")]
        [Min(0.02f)] public float repathInterval;
        [Min(0f)] public float attackExitRangeTolerance;

        [Header("Investigation")]
        [Min(0f)] public float investigateDuration;
        [Min(0f)] public float investigateTurnSpeed;

        [Header("Return")]
        [Min(0f)] public float returnStoppingDistance;
    }

    [Serializable]
    public struct EnemyCombatSettings
    {
        public WeaponConfig startingWeapon;
        [Min(0f)] public float aimTurnSpeed;
        [Min(0f)] public float reactionDuration;
        public bool requireLineOfSightToAttack;
    }

    [CreateAssetMenu(
        fileName = "EnemyConfig",
        menuName = "Game/AI/Enemy Config")]
    public sealed class EnemyConfig : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string enemyId;
        [SerializeField] private string displayName;

        [Header("State Graph")]
        [SerializeField] private EnemyStateGraph stateGraph;

        [Header("Settings")]
        [SerializeField] private EnemyVitalsSettings vitals;
        [SerializeField] private EnemyMovementSettings movement;
        [SerializeField] private EnemyPerceptionSettings perception;
        [SerializeField] private EnemyBehaviourSettings behaviour;
        [SerializeField] private EnemyCombatSettings combat;

        public string EnemyId => enemyId;
        public string DisplayName => displayName;
        public EnemyStateGraph StateGraph => stateGraph;

        public EnemyVitalsSettings Vitals => vitals;
        public EnemyMovementSettings Movement => movement;
        public EnemyPerceptionSettings Perception => perception;
        public EnemyBehaviourSettings Behaviour => behaviour;
        public EnemyCombatSettings Combat => combat;

        private void OnValidate()
        {
            vitals.maxHealth = Mathf.Max(1f, vitals.maxHealth);
            movement.patrolSpeed = Mathf.Max(0f, movement.patrolSpeed);
            movement.chaseSpeed = Mathf.Max(movement.patrolSpeed, movement.chaseSpeed);
            perception.viewDistance = Mathf.Max(0.1f, perception.viewDistance);
            perception.viewAngle = Mathf.Clamp(perception.viewAngle, 0f, 360f);
            behaviour.maxIdleDuration = Mathf.Max(behaviour.minIdleDuration, behaviour.maxIdleDuration);
        }
    }
}
