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

        private void Awake()
        {
            lastPosition = transform.position;
        }

        private void Update()
        {
            UpdateMovementSwing();
            UpdateAttackSwing();
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

        /// <summary>供编辑器工具引用各部位。</summary>
        public void AssignParts(Transform bodyP, Transform headP, Transform armLP, Transform armRP, Transform legLP, Transform legRP, Transform weaponP, Transform arrowP)
        {
            body = bodyP; head = headP; armL = armLP; armR = armRP; legL = legLP; legR = legRP;
            weaponPivot = weaponP; aimArrow = arrowP;
        }
    }
}
