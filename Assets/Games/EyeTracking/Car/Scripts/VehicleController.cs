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
        private const float IdleVolume = 0.15f;
        private const float RevVolume = 0.25f;
        private const float IdlePitch = 0.8f;
        private const float RevPitch = 0.9f;
        private const float GazeDeadZone = 0.15f;
        private const float GazeFullAt = 0.7f;

        [Header("Movement")]
        [SerializeField] private float accelerationForce = 800f;
        [SerializeField] private float steeringTorque = 30f;
        [SerializeField] private float maxSpeed = 20f;

        [Header("References")]
        [SerializeField] private Transform centerOfMass;

        private readonly GazeDwell shift = new();

        private Rigidbody body;
        private AudioSource engine;
        private CarHud hud;
        private VehicleModel model;
        private Gear gear;
        private bool directionMode;
        private float gazeThrottle;
        private float gazeSteering;
        private float throttleInput;
        private float steeringInput;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            hud = new CarHud(UiKit.Shared, transform);
            model = new VehicleModel(transform);
            engine = gameObject.AddComponent<AudioSource>();
            engine.clip = GameSounds.Clip(SoundCue.Engine);
            engine.loop = true;
            engine.playOnAwake = false;
            engine.spatialBlend = 0f;
            GameAudio.Route(engine, AudioChannel.Effects);

            if (centerOfMass != null)
                body.centerOfMass = transform.InverseTransformPoint(centerOfMass.position);
        }

        private void OnEnable()
        {
            gear = Gear.Stop;
            gazeThrottle = 0f;
            gazeSteering = 0f;
            shift.Reset();
            hud.SetMode(directionMode);
            hud.SetVisible(true);
            engine.Play();
        }

        private void OnDisable()
        {
            throttleInput = 0f;
            steeringInput = 0f;
            hud.SetVisible(false);
            engine.Stop();
        }

        private void Update()
        {
            if (!UiFocus.IsFree)
                return;

            if (KeyBinds.Down(KeyAction.EyeDrivingMode))
            {
                directionMode = !directionMode;
                gear = Gear.Stop;
                shift.Reset();
                hud.SetMode(directionMode);
            }

            if (directionMode)
                ReadDirections();
            else
                ReadScreenPosition();

            float keySteering = LayoutInput.GetHorizontalAxis();
            steeringInput = (keySteering != 0f ? keySteering : gazeSteering) * SteeringScale;

            float keyThrottle = LayoutInput.GetVerticalAxis();
            throttleInput = keyThrottle != 0f ? keyThrottle : gazeThrottle;

            if (directionMode)
                hud.Set(gear, shift.Direction, shift.Progress, Math.Sign(steeringInput), null, Time.deltaTime);
            else
                hud.Set((Gear)Math.Sign(throttleInput), Math.Sign(throttleInput), Mathf.Abs(throttleInput), Math.Sign(steeringInput), UFeelAPI.EyeTracking.Gaze, Time.deltaTime);

            model.Steer(steeringInput / SteeringScale);
            float revs = Mathf.Clamp01(body.linearVelocity.magnitude / maxSpeed);
            engine.volume = IdleVolume + (RevVolume * revs);
            engine.pitch = IdlePitch + (RevPitch * revs);
        }

        private static float Zone(float value)
        {
            return Mathf.Sign(value) * Mathf.Clamp01((Mathf.Abs(value) - GazeDeadZone) / (GazeFullAt - GazeDeadZone));
        }

        private void ReadDirections()
        {
            if (UFeelAPI.EyeTracking.IsBlinking)
            {
                gazeSteering = 0f;
                return;
            }

            gear = (Gear)Mathf.Clamp((int)gear + shift.Tick(GazeInput.VerticalDirection(), Time.deltaTime), (int)Gear.Reverse, (int)Gear.Drive);
            gazeThrottle = (int)gear;
            gazeSteering = GazeInput.HorizontalDirection();
        }

        private void ReadScreenPosition()
        {
            if (UFeelAPI.EyeTracking.IsBlinking)
                return;

            Vector2 gaze = UFeelAPI.EyeTracking.Gaze;
            gazeSteering = Zone(gaze.x);
            gazeThrottle = Zone(-gaze.y);
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
