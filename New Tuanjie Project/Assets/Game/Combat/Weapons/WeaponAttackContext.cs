using UnityEngine;

namespace FMBG.Combat
{
    /// <summary>一次攻击请求的上下文。</summary>
    public readonly struct WeaponAttackContext
    {
        public readonly Vector3 TargetPosition;
        public readonly Transform Target;

        public WeaponAttackContext(Vector3 targetPosition, Transform target = null)
        {
            TargetPosition = targetPosition;
            Target = target;
        }
    }
}
