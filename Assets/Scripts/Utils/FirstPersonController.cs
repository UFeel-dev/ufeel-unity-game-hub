using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float MoveSpeed = 5f;

    [Header("Look Settings")]
    public Transform CameraRoot;
    public float MouseSensitivity = 2f;
    private float pitch = 0f;
    private bool isUsingMouseToMoveView = true;

    [Header("Gravity")]
    public float Gravity = -9.81f;
    private float verticalVelocity;
    public CharacterController Controller;

    private void Start()
    {
        Controller = gameObject.AddComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMovement();

        if (isUsingMouseToMoveView)
        {
            HandleMouseLook();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleCursor();
        }
    }

    private void ToggleCursor()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        isUsingMouseToMoveView = !isUsingMouseToMoveView;
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = (transform.right * moveX) + (transform.forward * moveZ);
        verticalVelocity += Gravity * Time.deltaTime;

        move.y = verticalVelocity;

        Controller.Move(MoveSpeed * Time.deltaTime * move);
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * MouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity;

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -89f, 89f);

        CameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}
