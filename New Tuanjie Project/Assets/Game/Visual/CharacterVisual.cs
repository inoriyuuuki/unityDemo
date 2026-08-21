using UnityEngine;

namespace FMBG.Visual
{
    /// <summary>
    /// 程序化角色视觉表现：低多边形小人（身体/头/四肢）+ 朝向指示箭头 + 移动摆动/攻击挥砍。
    /// 通过编辑器工具 Game/Tools/Build Character Visual 一键构建。
    /// </summary>
    public sealed class CharacterVisual : MonoBehaviour
    {
        [Header("Body Parts")]
        [SerializeField] private Transform body;
        [SerializeField] private Transform head;
        [SerializeField] private Transform armL;
        [SerializeField] private Transform armR;
        [SerializeField] private Transform legL;
        [SerializeField] private Transform legR;
        [SerializeField] private Transform weaponPivot;   // 武器挂点（右手）
        [SerializeField] private Transform aimArrow;      // 朝向指示箭头

        [Header("Colors")]
        public Color bodyColor = new(0.2f, 0.6f, 1f);
        public Color headColor = new(0.95f, 0.85f, 0.7f);
        public Color limbColor = new(0.15f, 0.45f, 0.8f);

        [Header("Animation")]
        [SerializeField] private float walkSpeed = 6f;
        [SerializeField] private float walkAmplitude = 25f;
        [SerializeField] private float attackSwingDuration = 0.25f;
        [SerializeField] private float attackSwingAngle = 70f;

        private float swingTimer = -1f;
        private bool wasAttacking;
        private Vector3 lastPosition;
        private float moveAmount;

        // 技能攻击动画（由 Slate AttackAnimationClip 触发）
        private float attackAnimTimer = -1f;
        private float attackAnimDuration = 0.4f;
        private Vector3 armBaseRotation;   // 记录手臂初始旋转，动画结束恢复

        private void Awake()
        {
            lastPosition = transform.position;
            if (armR != null)
            {
                armBaseRotation = armR.localEulerAngles;
            }
        }

        private void Update()
        {
            UpdateMovementSwing();
            UpdateAttackSwing();
            UpdateSkillAttackAnimation();
        }

        private void UpdateMovementSwing()
        {
            if (armL == null || armR == null || legL == null || legR == null)
            {
                return;
            }

            float speed = (transform.position - lastPosition).magnitude / Mathf.Max(0.001f, Time.deltaTime);
            lastPosition = transform.position;
            moveAmount = Mathf.Lerp(moveAmount, Mathf.Clamp01(speed / walkSpeed), Time.deltaTime * 8f);

            float phase = Time.time * 10f * moveAmount;
            float swing = Mathf.Sin(phase) * walkAmplitude * moveAmount;

            armL.localRotation = Quaternion.Euler(swing, 0f, 0f);
            armR.localRotation = Quaternion.Euler(-swing, 0f, 0f);
            legL.localRotation = Quaternion.Euler(-swing, 0f, 0f);
            legR.localRotation = Quaternion.Euler(swing, 0f, 0f);
        }

        private void UpdateAttackSwing()
        {
            if (weaponPivot == null)
            {
                return;
            }

            var combat = GetComponent<FMBG.Combat.CharacterCombat>();
            bool attacking = combat != null && combat.IsAttacking;

            if (attacking && !wasAttacking)
            {
                swingTimer = 0f;
            }
            wasAttacking = attacking;

            if (swingTimer >= 0f)
            {
                swingTimer += Time.deltaTime;
                float t = Mathf.Clamp01(swingTimer / attackSwingDuration);

                // 挥砍：绕 Y 轴左右挥 + 轻微下压
                float angle = Mathf.Lerp(-attackSwingAngle, attackSwingAngle, t);
                weaponPivot.localRotation = Quaternion.Euler(-20f * (1f - t), angle, 0f);

                if (swingTimer >= attackSwingDuration)
                {
                    swingTimer = -1f;
                    weaponPivot.localRotation = Quaternion.identity;
                }
            }
            else
            {
                weaponPivot.localRotation = Quaternion.identity;
            }
        }

        /// <summary>
        /// 播放一次攻击动画（由 Slate 技能时间轴 AttackAnimationClip 触发）：
        /// 右臂（持武器）抬起→前挥→收回，身体轻微前倾，武器同步挥砍。
        /// </summary>
        public void PlayAttackAnimation(float duration)
        {
            attackAnimDuration = Mathf.Max(0.1f, duration);
            attackAnimTimer = 0f;
        }

        private void UpdateSkillAttackAnimation()
        {
            if (attackAnimTimer < 0f)
            {
                return;
            }

            attackAnimTimer += Time.deltaTime;
            float t = Mathf.Clamp01(attackAnimTimer / attackAnimDuration);
            if (t >= 1f)
            {
                attackAnimTimer = -1f;
                // 恢复初始姿态
                if (body != null) body.localRotation = Quaternion.identity;
                if (armR != null) armR.localRotation = Quaternion.Euler(armBaseRotation);
                if (weaponPivot != null) weaponPivot.localRotation = Quaternion.identity;
                return;
            }

            // 手臂动画：抬起(0-0.3) → 前挥(0.3-0.7) → 收回(0.7-1)
            float armPitch;
            if (t < 0.3f)
            {
                armPitch = Mathf.Lerp(0f, -120f, t / 0.3f);       // 抬起
            }
            else if (t < 0.7f)
            {
                armPitch = Mathf.Lerp(-120f, 30f, (t - 0.3f) / 0.4f); // 前挥
            }
            else
            {
                armPitch = Mathf.Lerp(30f, 0f, (t - 0.7f) / 0.3f);   // 收回
            }

            if (armR != null)
            {
                armR.localRotation = Quaternion.Euler(armPitch, 0f, 0f);
            }

            // 身体前倾（前挥阶段）
            if (body != null)
            {
                float lean = t >= 0.3f && t < 0.7f ? -15f * (1f - Mathf.Abs(t - 0.5f) * 2f) : 0f;
                body.localRotation = Quaternion.Euler(lean, 0f, 0f);
            }

            // 武器同步挥砍
            if (weaponPivot != null)
            {
                float swingAngle = Mathf.Lerp(-70f, 70f, Mathf.Clamp01((t - 0.3f) / 0.4f));
                weaponPivot.localRotation = Quaternion.Euler(-20f * (1f - Mathf.Abs(t - 0.5f) * 2f), swingAngle, 0f);
            }
        }

        /// <summary>供编辑器工具引用各部位。</summary>
        public void AssignParts(Transform bodyP, Transform headP, Transform armLP, Transform armRP, Transform legLP, Transform legRP, Transform weaponP, Transform arrowP)
        {
            body = bodyP; head = headP; armL = armLP; armR = armRP; legL = legLP; legR = legRP;
            weaponPivot = weaponP; aimArrow = arrowP;
        }
    }
}
