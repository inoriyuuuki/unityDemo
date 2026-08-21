using FMBG.Combat;
using FMBG.Skills;
using Slate;
using Slate.ActionClips;
using UnityEngine;

namespace FMBG.SlateClips
{
    /// <summary>技能 Clip 基类：通过 actor 找到施法者的当前施法上下文。</summary>
    [Attachable(typeof(ActorActionTrack))]
    public abstract class SkillClipBase : ActorActionClip
    {
        /// <summary>当前施法上下文（预览模式可能为 null）。</summary>
        protected SkillExecutionContext GetContext()
        {
            if (actor == null || !Application.isPlaying)
            {
                return null;
            }

            var caster = actor.GetComponentInParent<CharacterSkillController>();
            return caster != null ? caster.CurrentContext : null;
        }

        /// <summary>施法者（施法角色）。</summary>
        protected CharacterSkillController GetCaster()
        {
            if (actor == null)
            {
                return null;
            }

            return actor.GetComponentInParent<CharacterSkillController>();
        }

        /// <summary>真实结算保护：非运行时不执行伤害/生成等一次性逻辑。</summary>
        protected bool CanResolve => Application.isPlaying;
    }
}
