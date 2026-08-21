using UnityEngine;

namespace FMBG.Skills
{
    /// <summary>一次技能释放请求。</summary>
    public readonly struct SkillCastRequest
    {
        public readonly Vector3 TargetPosition;
        public readonly Transform Target;

        public SkillCastRequest(Vector3 targetPosition, Transform target = null)
        {
            TargetPosition = targetPosition;
            Target = target;
        }
    }
}
