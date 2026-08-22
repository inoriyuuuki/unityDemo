using UnityEngine;

namespace FMBG.Cameras
{
    /// <summary>俯视角固定偏移平滑跟随相机。</summary>
    public sealed class TopDownCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new(0f, 12f, -6f);
        [SerializeField] private float smoothTime = 0.25f;
        [SerializeField] private float lookAngle = 55f;
        [SerializeField] private float mouseOffsetStrength = 0.35f;

        private Vector3 velocity;
        private Camera controlledCamera;

        private void Awake()
        {
            controlledCamera = GetComponent<Camera>();
            if (target != null)
            {
                transform.position = target.position + offset;
                transform.rotation = Quaternion.Euler(lookAngle, 0f, 0f);
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desired = target.position + offset;
            transform.rotation = Quaternion.Euler(lookAngle, 0f, 0f);

            Vector3 mouseWorld = target.position;
            if (mouseOffsetStrength > 0f && controlledCamera != null)
            {
                Plane ground = new(Vector3.up, target.position);
                Ray ray = controlledCamera.ScreenPointToRay(Input.mousePosition);
                if (ground.Raycast(ray, out float enter))
                {
                    mouseWorld = ray.GetPoint(enter);
                }
            }

            Vector3 toMouse = mouseWorld - target.position;
            toMouse.y = 0f;
            desired += toMouse * mouseOffsetStrength;

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desired,
                ref velocity,
                smoothTime);
        }
    }
}
