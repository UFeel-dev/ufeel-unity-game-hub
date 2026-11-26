using System;
using GameHub.Common;
using UFeel;
using UnityEngine;

namespace GameHub.Car
{
    [RequireComponent(typeof(Rigidbody))]
    public class VehicleController : MonoBehaviour
    {
        private const float SteeringScale = 0.3f;
        private const float ReverseSpeedShare = 0.5f;
        private const float BrakeStrength = 2f;

        [Header("Movement")]
        [SerializeField] private float accelerationForce = 800f;
        [SerializeField] private float steeringTorque = 30f;
        [SerializeField] private float maxSpeed = 20f;

        [Header("References")]
        [SerializeField] private Transform centerOfMass;

        private readonly GazeDwell shift = new();

        private Rigidbody body;
        private CarHud hud;
        private Gear gear;
        private float throttleInput;
        private float steeringInput;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            hud = new CarHud(UiKit.Shared, transform);

            if (centerOfMass != null)
                body.centerOfMass = transform.InverseTransformPoint(centerOfMass.position);
        }

        private void OnEnable()
        {
            gear = Gear.Stop;
            shift.Reset();
            hud.SetVisible(true);
        }

        private void OnDisable()
        {
            throttleInput = 0f;
            steeringInput = 0f;
            hud.SetVisible(false);
        }

        private void Update()
        {
            if (!UiFocus.IsFree)
                return;

            bool blinking = UFeelAPI.EyeTracking.IsBlinking;
            if (!blinking)
                gear = (Gear)Mathf.Clamp((int)gear + shift.Tick(GazeInput.VerticalDirection(), Time.deltaTime), (int)Gear.Reverse, (int)Gear.Drive);

            float keySteering = LayoutInput.GetHorizontalAxis();
            float gazeSteering = blinking ? 0f : GazeInput.HorizontalDirection();
            steeringInput = (keySteering != 0f ? keySteering : gazeSteering) * SteeringScale;

            float keyThrottle = LayoutInput.GetVerticalAxis();
            throttleInput = keyThrottle != 0f ? keyThrottle : (int)gear;

            hud.Set(gear, shift.Direction, shift.Progress, Math.Sign(steeringInput));
        }

        private void FixedUpdate()
        {
            Vector3 velocity = body.linearVelocity;
            float speed = Vector3.Dot(velocity, transform.forward);
            if (throttleInput == 0f)
                body.AddForce(-BrakeStrength * new Vector3(velocity.x, 0f, velocity.z), ForceMode.Acceleration);
            else if (throttleInput > 0f ? speed < maxSpeed : speed > -maxSpeed * ReverseSpeedShare)
                body.AddForce(accelerationForce * throttleInput * Time.fixedDeltaTime * transform.forward);

            Quaternion turn = Quaternion.Euler(0f, steeringInput * steeringTorque * Time.fixedDeltaTime, 0f);
            body.MoveRotation(body.rotation * turn);
        }
    }
}
