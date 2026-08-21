using FMBG.Combat;
using UnityEngine;

namespace FMBG.Characters
{
    /// <summary>玩家攻击输入：鼠标左键 → CharacterCombat.TryAttack。</summary>
    public sealed class PlayerCombatInput : MonoBehaviour
    {
        [SerializeField] private CharacterCombat combat;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask groundLayer = ~0;

        private void Update()
        {
            if (combat == null)
            {
                return;
            }

            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    return;
                }
            }

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
            {
                combat.TryAttack(hit.point);
            }
            else
            {
                combat.TryAttack(transform.position + transform.forward * 5f);
            }
        }
    }
}
