using System;
using FMBG.Combat;
using UnityEngine;

namespace FMBG.AI
{
    /// <summary>可配置的状态切换条件类型。</summary>
    public enum TransitionConditionType
    {
        /// <summary>目标可见（CanSeeTarget）</summary>
        TargetVisible,

        /// <summary>目标丢失（!CanSeeTarget）</summary>
        TargetLost,

        /// <summary>目标在攻击范围内（可带容差）</summary>
        TargetInAttackRange,

        /// <summary>目标超出攻击范围（可带容差）</summary>
        TargetOutOfAttackRange,

        /// <summary>状态持续时间达到阈值（秒，使用黑板 StateTime）</summary>
        TimerElapsed,

        /// <summary>到达导航目的地</summary>
        ReachedDestination,

        /// <summary>生命值低于阈值（0~1 比例）</summary>
        HealthBelow,

        /// <summary>总是满足（无条件切换）</summary>
        Always
    }

    /// <summary>状态切换条件：可在 GraphView 编辑器中配置。</summary>
    [Serializable]
    public sealed class TransitionCondition
    {
        [SerializeField] private TransitionConditionType type = TransitionConditionType.Always;

        [Header("Timer")]
        [SerializeField] private float duration = 2f;

        [Header("Range")]
        [SerializeField] private float tolerance = 0.4f;

        [Header("Health")]
        [SerializeField, Range(0f, 1f)] private float healthThreshold = 0.3f;

        public TransitionConditionType Type => type;
        public float Duration => duration;
        public float Tolerance => tolerance;
        public float HealthThreshold => healthThreshold;

        public TransitionCondition() { }

        public TransitionCondition(TransitionConditionType type)
        {
            this.type = type;
        }

        /// <summary>供编辑器设置条件类型。</summary>
        public void SetType(TransitionConditionType value) => type = value;

        /// <summary>供编辑器/工厂设置阈值参数。</summary>
        public void SetDuration(float value) => duration = Mathf.Max(0f, value);
        public void SetTolerance(float value) => tolerance = Mathf.Max(0f, value);
        public void SetHealthThreshold(float value) => healthThreshold = Mathf.Clamp01(value);

        public bool Evaluate(EnemyContext context)
        {
            if (context == null)
            {
                return false;
            }

            switch (type)
            {
                case TransitionConditionType.TargetVisible:
                    return context.Perception != null && context.Perception.CanSeeTarget;

                case TransitionConditionType.TargetLost:
                    return context.Perception == null || !context.Perception.CanSeeTarget;

                case TransitionConditionType.TargetInAttackRange:
                {
                    Transform target = context.Perception != null ? context.Perception.Target : null;
                    return target != null && context.Combat.IsTargetInAttackRange(target, tolerance);
                }

                case TransitionConditionType.TargetOutOfAttackRange:
                {
                    Transform target = context.Perception != null ? context.Perception.Target : null;
                    if (target == null)
                    {
                        return false;
                    }
                    return !context.Combat.IsTargetInAttackRange(target, tolerance);
                }

                case TransitionConditionType.TimerElapsed:
                    return context.Blackboard.StateTime >= duration;

                case TransitionConditionType.ReachedDestination:
                    return context.Motor != null && context.Motor.ReachedDestination();

                case TransitionConditionType.HealthBelow:
                {
                    if (context.Health == null)
                    {
                        return false;
                    }
                    float ratio = context.Health.CurrentHealth / context.Health.MaxHealth;
                    return ratio <= healthThreshold;
                }

                case TransitionConditionType.Always:
                default:
                    return true;
            }
        }

        public string Describe()
        {
            switch (type)
            {
                case TransitionConditionType.TargetVisible: return "目标可见";
                case TransitionConditionType.TargetLost: return "目标丢失";
                case TransitionConditionType.TargetInAttackRange: return $"进入攻击范围(容差{tolerance})";
                case TransitionConditionType.TargetOutOfAttackRange: return $"超出攻击范围(容差{tolerance})";
                case TransitionConditionType.TimerElapsed: return $"等待 {duration}s";
                case TransitionConditionType.ReachedDestination: return "到达目的地";
                case TransitionConditionType.HealthBelow: return $"生命低于 {healthThreshold:P0}";
                case TransitionConditionType.Always: return "总是";
                default: return "未知";
            }
        }
    }
}
