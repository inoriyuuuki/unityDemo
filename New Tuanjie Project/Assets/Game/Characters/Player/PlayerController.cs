using UnityEngine;

namespace FMBG.Characters
{
    /// <summary>玩家移动与鼠标朝向控制。</summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float turnSpeed = 720f;

        [Header("Ground")]
        [SerializeField] private LayerMask groundLayer = ~0;
        [SerializeField] private float maxRayDistance = 100f;

        private CharacterController controller;
        private Camera mainCamera;
        private Vector3 moveInput;

        public Vector3 MoveInput => moveInput;
        public bool MovementLocked { get; private set; }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            mainCamera = Camera.main;
        }

        private void Update()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            moveInput = new Vector3(h, 0f, v).normalized;

            if (MovementLocked)
            {
                return;
            }

            Move();
            FaceMouse();
        }

        public void SetMovementLocked(bool locked)
        {
            MovementLocked = locked;
        }

        private void Move()
        {
            Vector3 worldDirection = CameraRelativeDirection(moveInput);
            Vector3 motion = worldDirection * (moveSpeed * Time.deltaTime);
            controller.Move(motion);
        }

        private Vector3 CameraRelativeDirection(Vector3 input)
        {
            if (mainCamera == null)
            {
                return input;
            }

            Vector3 forward = mainCamera.transform.forward;
            Vector3 right = mainCamera.transform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            return (forward * input.z + right * input.x).normalized;
        }

        private void FaceMouse()
        {
            if (mainCamera == null)
            {
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, groundLayer))
            {
                Vector3 direction = hit.point - transform.position;
                direction.y = 0f;
                if (direction.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(
                        transform.rotation,
                        targetRotation,
                        turnSpeed * Time.deltaTime);
                }
            }
        }
    }
}
