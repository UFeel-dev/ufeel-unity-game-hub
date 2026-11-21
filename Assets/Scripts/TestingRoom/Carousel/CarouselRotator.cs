using UnityEngine;

public class CarouselRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 0.5f;
    public bool isRotatingContinuously = true;
    public float radius = 19.7f;
    public Transform doorHoldersParent;

    [Header("Target Rotation")]
    public bool rotateToTarget;
    public float targetAngle;
    public float rotationSmoothSpeed = 3f;

    private static float acceleratedRotationSpeedMultiplier = 5f;
    private bool rotationSpeedAccerelated = false;

    private Transform[] doorHolders;
    private float currentAngle;

    private void Start()
    {
        doorHolders = new Transform[doorHoldersParent.childCount];

        int i = 0;
        foreach (Transform child in doorHoldersParent)
        {
            if (child.CompareTag("DoorHolder"))
            {
                doorHolders[i] = child;
                i++;
            }
        }

        PositionDoors();
    }

    private void Update()
    {
        if (rotateToTarget)
        {
            RotateToAngle(targetAngle);
        }
        else if (isRotatingContinuously)
        {
            currentAngle += rotationSpeed * Time.deltaTime * 360f / 20f;
            PositionDoors();
        }

        HandleInput();
    }

    private void HandleInput()
    {
        // if (Input.GetKeyDown(KeyCode.R))
        // {
        //     isRotatingContinuously = !isRotatingContinuously;
        // }

        if (LayoutInput.GetKeyDown(KeyCode.F))
        {
            ToggleRotationSpeedAcceleration();
        }
    }

    private void ToggleRotationSpeedAcceleration()
    {
        rotationSpeedAccerelated = !rotationSpeedAccerelated;
    }

    private void PositionDoors()
    {
        float angleStep = 360f / doorHolders.Length;

        for (int i = 0; i < doorHolders.Length; i++)
        {
            float angle = (i * angleStep) + currentAngle;
            float angleRad = Mathf.Deg2Rad * angle;
            float x = Mathf.Sin(angleRad) * radius;
            float z = Mathf.Cos(angleRad) * radius;

            doorHolders[i].localPosition = new Vector3(x, doorHolders[i].localPosition.y, z);
        }
    }

    public void RotateCarousel(float amount)
    {
        currentAngle += amount;
        PositionDoors();
    }

    public void SetIsRotating(bool shouldRotate)
    {
        isRotatingContinuously = shouldRotate;
    }

    public void RotateToAngle(float angle)
    {
        float actualRotationSpeed = rotationSpeedAccerelated ? rotationSmoothSpeed * acceleratedRotationSpeedMultiplier : rotationSmoothSpeed;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, angle, actualRotationSpeed * Time.deltaTime);
        currentAngle = newAngle;
        PositionDoors();

        if (Mathf.Approximately(newAngle, angle))
        {
            rotateToTarget = false;
        }
    }

    public void TriggerRotateTo(float angle)
    {
        targetAngle = angle;
        rotateToTarget = true;
        isRotatingContinuously = false;
    }

    public void RotateToDoor(int doorIndex)
    {
        float angleStep = 360f / doorHolders.Length;
        float targetDoorAngle = -doorIndex * angleStep;
        TriggerRotateTo(targetDoorAngle);
    }
}
