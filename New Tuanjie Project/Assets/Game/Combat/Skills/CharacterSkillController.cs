using System.Collections.Generic;
using FMBG.Characters;
using FMBG.Combat;
using UnityEngine;

namespace FMBG.Skills
{
    /// <summary>技能控制器：释放、冷却、范围校验、打断、移动锁定。</summary>
    public sealed class CharacterSkillController : MonoBehaviour
    {
        [SerializeField] private CharacterCombat combat;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private SlateSkillPlayer slatePlayer;

        private readonly Dictionary<string, float> cooldowns = new();

        public bool IsCasting => slatePlayer != null && slatePlayer.IsPlaying;

        public SkillExecutionContext CurrentContext => slatePlayer != null ? slatePlayer.CurrentContext : null;

        public bool TryCast(SkillConfig skill, SkillCastRequest request)
        {
            return TryCast(skill, request, null);
        }

        /// <summary>
        /// 释放技能。可指定用于伤害结算的武器配置（玩家双武器：左键近战/右键远程）。
        /// </summary>
        public bool TryCast(SkillConfig skill, SkillCastRequest request, WeaponConfig weaponOverride)
        {
            if (skill == null || IsCasting || IsOnCooldown(skill))
            {
                return false;
            }

            if (!IsInRange(skill, request))
            {
                return false;
            }

            if (skill.FaceTarget && combat != null)
            {
                combat.FaceTowards(request.TargetPosition);
            }

            WeaponConfig weapon = weaponOverride != null
                ? weaponOverride
                : (combat != null ? combat.CurrentWeaponConfig : null);

            var context = new SkillExecutionContext(
                this,
                combat,
                skill,
                weapon,
                request.Target,
                request.TargetPosition);

            if (skill.LockMovement && playerController != null)
            {
                playerController.SetMovementLocked(true);
            }

            slatePlayer.Play(context, () => FinishSkill(context));

            return true;
        }

        private void FinishSkill(SkillExecutionContext context)
        {
            if (context != null && context.Skill != null)
            {
                cooldowns[context.Skill.SkillId] = Time.time + context.Skill.Cooldown;
            }

            if (playerController != null)
            {
                playerController.SetMovementLocked(false);
            }
        }

        public void Interrupt()
        {
            if (slatePlayer != null)
            {
                slatePlayer.Stop();
            }

            if (playerController != null)
            {
                playerController.SetMovementLocked(false);
            }
        }

        /// <summary>供 Slate Clip 设置移动锁定（LockMovementClip 等）。</summary>
        public void SetMovementLock(bool locked)
        {
            if (playerController != null)
            {
                playerController.SetMovementLocked(locked);
            }
        }

        public bool IsOnCooldown(SkillConfig skill)
        {
            if (skill == null || !cooldowns.TryGetValue(skill.SkillId, out float endTime))
            {
                return false;
            }

            return Time.time < endTime;
        }

        /// <summary>冷却剩余秒数（供 UI 显示）。</summary>
        public float GetCooldownRemaining(SkillConfig skill)
        {
            if (skill == null || !cooldowns.TryGetValue(skill.SkillId, out float endTime))
            {
                return 0f;
            }

            return Mathf.Max(0f, endTime - Time.time);
        }

        private bool IsInRange(SkillConfig skill, SkillCastRequest request)
        {
            float distance = Vector3.Distance(transform.position, request.TargetPosition);
            if (skill.MinCastRange > 0f && distance < skill.MinCastRange)
            {
                return false;
            }

            if (skill.MaxCastRange > 0f && distance > skill.MaxCastRange)
            {
                return false;
            }

            return true;
        }
    }
}
