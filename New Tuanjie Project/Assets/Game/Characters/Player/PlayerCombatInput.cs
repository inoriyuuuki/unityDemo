using FMBG.Combat;
using FMBG.Skills;
using UnityEngine;

namespace FMBG.Characters
{
    /// <summary>
    /// 玩家攻击输入：左键近战（剑技能），右键远程（手枪技能）。
    /// 通过 CharacterSkillController 释放，含冷却。
    /// </summary>
    public sealed class PlayerCombatInput : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterSkillController skillController;
        [SerializeField] private Health health;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask groundLayer = ~0;

        [Header("Weapons / Skills")]
        [SerializeField] private WeaponConfig meleeWeapon;   // 剑
        [SerializeField] private WeaponConfig rangedWeapon;  // 手枪
        [SerializeField] private SkillConfig meleeSkill;     // 近战技能（挥剑斩）
        [SerializeField] private SkillConfig rangedSkill;    // 远程技能（手枪射击）

        public SkillConfig MeleeSkill => meleeSkill;
        public SkillConfig RangedSkill => rangedSkill;

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<Health>();
            }
        }

        private void Update()
        {
            if (skillController == null || health == null || !health.IsAlive)
            {
                return;
            }

            Vector3 targetPos = GetGroundTarget();

            // 左键：近战
            if (Input.GetMouseButtonDown(0) && meleeSkill != null)
            {
                skillController.TryCast(meleeSkill, new SkillCastRequest(targetPos, null), meleeWeapon);
            }

            // 右键：远程
            if (Input.GetMouseButtonDown(1) && rangedSkill != null)
            {
                skillController.TryCast(rangedSkill, new SkillCastRequest(targetPos, null), rangedWeapon);
            }
        }

        private Vector3 GetGroundTarget()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (mainCamera != null)
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
                {
                    return hit.point;
                }
            }

            return transform.position + transform.forward * 5f;
        }
    }
}
