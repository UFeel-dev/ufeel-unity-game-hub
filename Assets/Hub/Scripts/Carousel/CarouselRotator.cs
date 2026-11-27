using System.Collections.Generic;
using GameHub.Common;
using UnityEngine;

namespace GameHub.Hub
{
    public class CarouselRotator : MonoBehaviour
    {
        private const float FullTurnSeconds = 20f;
        private const float AcceleratedSpeedMultiplier = 5f;

        [Header("Layout")]
        [SerializeField] private Transform doorHoldersParent;
        [SerializeField] private float radius = 19.7f;

        [Header("Rotation")]
        [SerializeField] private float rotationSpeed = 0.5f;
        [SerializeField] private bool isRotatingContinuously = true;
        [SerializeField] private float rotationSmoothSpeed = 3f;

        private Transform[] doorHolders;
        private float currentAngle;
        private float targetAngle;
        private bool isAccelerated;

        public bool IsRotatingToTarget { get; private set; }

        private float AngleStep => 360f / doorHolders.Length;

        private void Start()
        {
            List<Transform> holders = new();
            foreach (Transform child in doorHoldersParent)
            {
                if (child.CompareTag(Tags.DoorHolder))
                    holders.Add(child);
            }

            doorHolders = holders.ToArray();
            PositionDoors();
        }

        private void Update()
        {
            if (IsRotatingToTarget)
            {
                RotateTowardsTarget();
            }
            else if (isRotatingContinuously)
            {
                currentAngle += rotationSpeed * Time.deltaTime * 360f / FullTurnSeconds;
                PositionDoors();
            }

            if (LayoutInput.GetKeyDown(KeyCode.F))
                isAccelerated = !isAccelerated;
        }

        public void RotateToDoor(int doorIndex)
        {
            targetAngle = -doorIndex * AngleStep;
            IsRotatingToTarget = true;
            isRotatingContinuously = false;
        }

        private void RotateTowardsTarget()
        {
            float speed = isAccelerated ? rotationSmoothSpeed * AcceleratedSpeedMultiplier : rotationSmoothSpeed;
            currentAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, speed * Time.deltaTime);
            PositionDoors();

            if (Mathf.Approximately(currentAngle, targetAngle))
                IsRotatingToTarget = false;
        }

        private void PositionDoors()
        {
            for (int i = 0; i < doorHolders.Length; i++)
            {
                float degrees = (i * AngleStep) + currentAngle;
                float angle = Mathf.Deg2Rad * degrees;
                Vector3 position = doorHolders[i].localPosition;
                doorHolders[i].localPosition = new Vector3(Mathf.Sin(angle) * radius, position.y, Mathf.Cos(angle) * radius);
                doorHolders[i].localRotation = Quaternion.Euler(0f, degrees, 0f);
            }
        }
    }
}
