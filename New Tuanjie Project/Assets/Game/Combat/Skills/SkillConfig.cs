using Slate;
using UnityEngine;

namespace FMBG.Skills
{
    public enum SkillTargetType
    {
        None,
        Direction,
        Position,
        Unit
    }

    /// <summary>技能静态配置：规则 + Slate 时间轴入口。</summary>
    [CreateAssetMenu(
        fileName = "SkillConfig",
        menuName = "Game/Combat/Skill Config")]
    public sealed class SkillConfig : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string skillId;
        [SerializeField] private string displayName;

        [Header("Slate")]
        [SerializeField] private Cutscene timelinePrefab;

        [Header("Cast")]
        [SerializeField] private SkillTargetType targetType;
        [SerializeField, UnityEngine.Min(0f)] private float cooldown;
        [SerializeField, UnityEngine.Min(0f)] private float minCastRange;
        [SerializeField, UnityEngine.Min(0f)] private float maxCastRange = 2f;

        [Header("Control")]
        [SerializeField] private bool lockMovement = true;
        [SerializeField] private bool faceTarget = true;
        [SerializeField] private bool canBeInterrupted = true;

        [Header("Damage")]
        [SerializeField, UnityEngine.Min(0f)] private float damageMultiplier = 1f;

        public string SkillId => skillId;
        public string DisplayName => displayName;
        public Cutscene TimelinePrefab => timelinePrefab;

        public SkillTargetType TargetType => targetType;
        public float Cooldown => cooldown;
        public float MinCastRange => minCastRange;
        public float MaxCastRange => maxCastRange;

        public bool LockMovement => lockMovement;
        public bool FaceTarget => faceTarget;
        public bool CanBeInterrupted => canBeInterrupted;
        public float DamageMultiplier => damageMultiplier;
    }
}
