using UnityEngine;
using UnityEngine.Rendering;

namespace GameHub.Common
{
    public class FirstPersonController : MonoBehaviour
    {
        private const float MaxPitch = 89f;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float gravity = -9.81f;

        [Header("Look")]
        [SerializeField] private Transform cameraRoot;
        [SerializeField] private float mouseSensitivity = 2f;

        private CharacterController controller;
        private float pitch;
        private float verticalVelocity;
        private bool isMouseLookEnabled = true;

        private void Awake()
        {
            if (!TryGetComponent(out controller))
                controller = gameObject.AddComponent<CharacterController>();
            if (TryGetComponent(out MeshRenderer body))
                body.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
        }

        private void Start()
        {
            SetCursorLocked(true);
        }

        private void Update()
        {
            if (UiFocus.IsCaptured)
                return;

            if (isMouseLookEnabled)
            {
                Move();
                Look();
            }

            if (LayoutInput.GetKeyDown(KeyCode.Tab))
            {
                isMouseLookEnabled = !isMouseLookEnabled;
                SetCursorLocked(isMouseLookEnabled);
            }
        }

        public void Teleport(Vector3 position)
        {
            controller.enabled = false;
            transform.position = position;
            controller.enabled = true;
        }

        private static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        private void Move()
        {
            Vector3 move = (transform.right * LayoutInput.GetHorizontalAxis()) + (transform.forward * LayoutInput.GetVerticalAxis());
            verticalVelocity += gravity * Time.deltaTime;
            move.y = verticalVelocity;

            controller.Move(moveSpeed * Time.deltaTime * move);
        }

        private void Look()
        {
            Vector2 delta = LayoutInput.GetMouseDelta() * (mouseSensitivity * GameSettings.LookMultiplier);

            pitch = Mathf.Clamp(pitch - delta.y, -MaxPitch, MaxPitch);
            cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            transform.Rotate(Vector3.up * delta.x);
        }
    }
}
