using UFeel;
using UnityEngine;

public sealed class CarEyeTrackingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _sphere;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private FloatUpDownInteractionDisabler interactionDisabler;

    [Header("Movement Settings")]
    [SerializeField] private float sphereDistance = 4f;
    [SerializeField] private Vector2 angularMovementRange = new(30f, 20f);
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Gaze Stability Settings")]
    [SerializeField] private float gazeHoldThreshold = 0.25f;
    [SerializeField] private float gazeReleaseCooldown = 0.5f;

    private float gazeTimer;
    private bool isGazing;
    private float timeSinceLostGaze = Mathf.Infinity;

    private void Awake()
    {
        UFeelDebugHUD.Clear();

        InitializeDetection();

        if (playerCamera == null)
        {
            playerCamera = FindFirstObjectByType<Camera>();
            if (playerCamera == null)
                Debug.LogWarning("CarEyeTrackingManager: No camera found in loaded scenes.");
        }
    }

    private static void InitializeDetection()
    {
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.V))
        {
            InitializeDetection();
        }
        if (playerCamera == null || _sphere == null) return;

        UpdateGazeStatus();

        if (!isGazing) return;

        Vector3 targetPosition = ComputeTargetPosition();
        MoveSphere(targetPosition);
    }

    private void UpdateGazeStatus()
    {
        if (IsLookingAtSphere())
        {
            gazeTimer += Time.deltaTime;
            timeSinceLostGaze = 0f;

            if (!isGazing && gazeTimer >= gazeHoldThreshold)
            {
                isGazing = true;
                if (interactionDisabler)
                    interactionDisabler.OnInteract();
            }
        }
        else
        {
            gazeTimer = 0f;
            timeSinceLostGaze += Time.deltaTime;

            if (isGazing && timeSinceLostGaze >= gazeReleaseCooldown)
            {
                isGazing = false;
            }
        }
    }

    private bool IsLookingAtSphere()
    {
        // Ray ray = new(playerCamera.transform.position, playerCamera.transform.forward);
        // return Physics.Raycast(ray, out RaycastHit hit, 100f) && hit.collider?.gameObject == _sphere.gameObject;

        if (_sphere == null || playerCamera == null)
            return false;

        Vector3 toSphere = _sphere.position - playerCamera.transform.position;
        float angleToSphere = Vector3.Angle(playerCamera.transform.forward, toSphere.normalized);

        float cameraFOV = playerCamera.fieldOfView;

        return angleToSphere < cameraFOV / 2f;
    }

    private Vector3 ComputeTargetPosition()
    {
        GazeDirection directions = UFeelAPI.Gaze.CurrentDirections;

        Vector2 direction = Vector2.zero;
        if (directions.IsDirection(GazeDirection.Left)) direction.x--;
        if (directions.IsDirection(GazeDirection.Right)) direction.x++;
        // if (directions.up) direction.y++;
        // if (directions.down) direction.y--;

        if (direction != Vector2.zero && !directions.IsDirection(GazeDirection.Center))
        {
            direction.Normalize();
        }

        Quaternion offset = Quaternion.Euler(
            direction.y * angularMovementRange.y,
            direction.x * angularMovementRange.x,
            0f
        );

        Vector3 directionFromCamera = offset * playerCamera.transform.forward;

        Vector3 toSphere = _sphere.position - playerCamera.transform.position;
        float currentDistance = toSphere.magnitude;

        if (currentDistance < sphereDistance)
        {
            return playerCamera.transform.position + (directionFromCamera.normalized * sphereDistance);
        }

        return playerCamera.transform.position + (directionFromCamera.normalized * currentDistance);
    }

    private void MoveSphere(Vector3 targetPosition)
    {
        _sphere.position = Vector3.Lerp(_sphere.position, targetPosition, Time.deltaTime * smoothSpeed);
    }
}
