using FMBG.Combat;
using FMBG.Skills;
using UnityEngine;

namespace FMBG.Timeline
{
    /// <summary>一次播放中传递给 Clip 的执行上下文（每次播放独立创建）。</summary>
    public sealed class SkillClipContext
    {
        /// <summary>施法上下文（编辑器预览时为 null）。</summary>
        public SkillExecutionContext SkillContext { get; }

        /// <summary>绑定 Actor（施法者）。</summary>
        public Transform Actor { get; }

        /// <summary>是否为编辑器预览（预览禁止真实结算）。</summary>
        public bool IsPreview { get; }

        public SkillClipContext(SkillExecutionContext skillContext, Transform actor, bool isPreview = false)
        {
            SkillContext = skillContext;
            Actor = actor;
            IsPreview = isPreview;
        }

        /// <summary>施法者控制器。</summary>
        public CharacterSkillController Caster => SkillContext != null ? SkillContext.Caster : null;

        /// <summary>施法者战斗组件。</summary>
        public CharacterCombat Combat => SkillContext != null ? SkillContext.Combat : null;

        /// <summary>真实结算保护：非运行时不执行伤害/生成等一次性逻辑。</summary>
        public bool CanResolve => Application.isPlaying && !IsPreview;
    }
}
