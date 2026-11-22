using GameHub.Common;
using UnityEngine;

namespace GameHub.Car
{
    [RequireComponent(typeof(Rigidbody))]
    public class VehicleController : MonoBehaviour
    {
        private const float GazeSteering = 0.3f;

        [Header("Movement")]
        [SerializeField] private float accelerationForce = 800f;
        [SerializeField] private float steeringTorque = 30f;
        [SerializeField] private float maxSpeed = 20f;

        [Header("References")]
        [SerializeField] private Transform centerOfMass;

        private Rigidbody body;
        private float throttleInput;
        private float steeringInput;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();

            if (centerOfMass != null)
                body.centerOfMass = transform.InverseTransformPoint(centerOfMass.position);
        }

        private void Update()
        {
            steeringInput = GazeInput.HorizontalDirection() * GazeSteering;
            throttleInput = LayoutInput.GetVerticalAxis();
        }

        private void FixedUpdate()
        {
            if (body.linearVelocity.magnitude < maxSpeed)
                body.AddForce(accelerationForce * throttleInput * Time.fixedDeltaTime * transform.forward);

            Quaternion turn = Quaternion.Euler(0f, steeringInput * steeringTorque * Time.fixedDeltaTime, 0f);
            body.MoveRotation(body.rotation * turn);
        }
    }
}
