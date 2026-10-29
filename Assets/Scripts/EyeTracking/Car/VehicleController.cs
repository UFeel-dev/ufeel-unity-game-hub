using UFeel;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class VehicleController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float accelerationForce = 800f;
    [SerializeField] private float steeringTorque = 30f;
    [SerializeField] private float maxSpeed = 20f;

    [Header("Engine Activation")]
    [SerializeField] private float sequenceTimeout = 2.5f;

    [Header("References")]
    [SerializeField] private Transform centerOfMass;
    [SerializeField] private Transform respawnPosition;

    private Rigidbody rb;

    private float throttleInput;
    private float steeringInput;

    private bool canMove = true;

    private enum ActivationStep
    {
        WaitBlink,
        WaitLeft,
        WaitRight,
        WaitFinalBlink
    }

    private ActivationStep activationStep = ActivationStep.WaitBlink;

    private float lastStepTime;

    private bool previousBlink;
    private bool previousLeft;
    private bool previousRight;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (centerOfMass != null)
            rb.centerOfMass = transform.InverseTransformPoint(centerOfMass.position);
    }

    private void Update()
    {
        GazeDirection currentDirections = UFeelAPI.Gaze.CurrentDirections;
        bool blinkStatus = UFeelAPI.Gaze.IsBlinking;

        UpdateSteering(currentDirections);

        throttleInput = canMove ? Input.GetAxis("Vertical") : 0f;

        HandleInputs();
    }

    private void HandleInputs()
    {
        // if (Input.GetKeyDown(KeyCode.R))
        // {
        //     Respawn();
        // }
    }

    private void FixedUpdate()
    {
        if (canMove && rb.linearVelocity.magnitude < maxSpeed)
        {
            rb.AddForce(
                accelerationForce * throttleInput * Time.fixedDeltaTime * transform.forward);
        }

        Quaternion turnRotation = Quaternion.Euler(
            0f,
            steeringInput * steeringTorque * Time.fixedDeltaTime,
            0f);

        rb.MoveRotation(rb.rotation * turnRotation);
    }

    private void UpdateSteering(GazeDirection directions)
    {
        if (directions.IsDirection(GazeDirection.Left) && !directions.IsDirection(GazeDirection.Right))
        {
            steeringInput = -0.3f;
        }
        else if (directions.IsDirection(GazeDirection.Right) && !directions.IsDirection(GazeDirection.Left))
        {
            steeringInput = 0.3f;
        }
        else
        {
            steeringInput = 0f;
        }
    }

    private void UpdateEngineSequence(GazeDirection directions, bool? blinkStatus)
    {
        if (activationStep != ActivationStep.WaitBlink &&
            Time.time - lastStepTime > sequenceTimeout)
        {
            ResetSequence();
        }

        bool blink = blinkStatus == true;
        bool blinkPressed = blink && !previousBlink;

        bool leftPressed = directions.IsDirection(GazeDirection.Left) && !previousLeft;
        bool rightPressed = directions.IsDirection(GazeDirection.Right) && !previousRight;

        previousBlink = blink;
        previousLeft = directions.IsDirection(GazeDirection.Left);
        previousRight = directions.IsDirection(GazeDirection.Right);

        if (canMove)
        {
            return;
        }

        switch (activationStep)
        {
            case ActivationStep.WaitBlink:
                if (blinkPressed)
                {
                    AdvanceSequence(ActivationStep.WaitLeft);
                }
                break;

            case ActivationStep.WaitLeft:
                if (leftPressed)
                {
                    AdvanceSequence(ActivationStep.WaitRight);
                }
                break;

            case ActivationStep.WaitRight:
                if (rightPressed)
                {
                    AdvanceSequence(ActivationStep.WaitFinalBlink);
                }
                break;

            case ActivationStep.WaitFinalBlink:
                if (blinkPressed)
                {
                    ToggleEngine();
                    ResetSequence();
                }
                break;
        }
    }

    private void AdvanceSequence(ActivationStep nextStep)
    {
        activationStep = nextStep;
        lastStepTime = Time.time;
    }

    private void ResetSequence()
    {
        activationStep = ActivationStep.WaitBlink;
        lastStepTime = 0f;
    }

    private void ToggleEngine()
    {
        canMove = !canMove;
    }

    private void Respawn()
    {
        rb.position = respawnPosition.position;
    }
}
