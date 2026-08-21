using UnityEngine;

namespace FMBG.Combat
{
    /// <summary>组件扩展：从自身或父级查找组件（含 Inactive）。</summary>
    public static class ComponentExtensions
    {
        public static bool TryGetComponentInParent<T>(this Component self, out T component)
            where T : class
        {
            component = self != null ? self.GetComponentInParent<T>() : null;
            return component != null;
        }
    }
}
