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
        [SerializeField] private SkillTimelinePlayer timelinePlayer;

        private readonly Dictionary<string, float> cooldowns = new();

        public bool IsCasting => timelinePlayer != null && timelinePlayer.IsPlaying;

        public SkillExecutionContext CurrentContext => timelinePlayer != null ? timelinePlayer.CurrentContext : null;

        public bool TryCast(SkillConfig skill, SkillCastRequest request)
        {
            return TryCast(skill, request, null);
        }

        /// <summary>
        /// 释放技能。可指定用于伤害结算的武器配置（玩家双武器：左键近战/右键远程）。
        /// </summary>
        public bool TryCast(SkillConfig skill, SkillCastRequest request, WeaponConfig weaponOverride)
        {
            if (skill == null)
            {
                return false;
            }

            if (IsCasting)
            {
                Debug.LogWarning($"[Skill] {skill.DisplayName} 释放失败：正在施法中。", this);
                return false;
            }

            if (IsOnCooldown(skill))
            {
                Debug.LogWarning($"[Skill] {skill.DisplayName} 释放失败：冷却中（剩余 {GetCooldownRemaining(skill):0.0}s）。", this);
                return false;
            }

            if (!IsInRange(skill, request))
            {
                Debug.LogWarning($"[Skill] {skill.DisplayName} 释放失败：超出施法范围（最大 {skill.MaxCastRange}m）。", this);
                return false;
            }

            // 冷却从施法开始时计时（而非动画结束后），CD UI 与手感更符合直觉
            cooldowns[skill.SkillId] = Time.time + skill.Cooldown;

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

            try
            {
                timelinePlayer.Play(context, () => FinishSkill(context));
            }
            catch (System.Exception e)
            {
                // 时间轴启动失败时清理状态，避免 IsCasting 永久卡死导致后续技能全部失效
                Debug.LogError($"[Skill] {skill.DisplayName} 时间轴启动失败：{e.Message}", this);
                if (cooldowns.ContainsKey(skill.SkillId))
                {
                    cooldowns.Remove(skill.SkillId);
                }

                if (playerController != null)
                {
                    playerController.SetMovementLocked(false);
                }

                if (timelinePlayer != null)
                {
                    timelinePlayer.Stop();
                }

                return false;
            }

            return true;
        }

        private void FinishSkill(SkillExecutionContext context)
        {
            // 冷却已在施法开始时登记（TryCast），这里仅负责解锁移动

            if (playerController != null)
            {
                playerController.SetMovementLocked(false);
            }
        }

        public void Interrupt()
        {
            if (timelinePlayer != null)
            {
                timelinePlayer.Stop();
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
            // 方向技能在没有锁定单位时只使用目标点决定朝向，不应因为鼠标点较远而拒绝施法。
            if (skill.TargetType == SkillTargetType.None ||
                (skill.TargetType == SkillTargetType.Direction && request.Target == null))
            {
                return true;
            }

            // 俯视角战斗按地面平面计算距离，避免角色根节点高度与地面点击点的 Y 差缩短有效范围。
            Vector3 delta = request.TargetPosition - transform.position;
            delta.y = 0f;
            float distance = delta.magnitude;

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
