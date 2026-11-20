using UFeel;
using UnityEngine;

public class DoorEyeController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DoorSelectionManager doorSelectionManager;

    [Header("Settings")]
    [SerializeField] private float doubleBlinkWindow = 0.5f;

    private bool previousBlink;

    private EyeTrackingDirection previousEyeTrackingDirection = EyeTrackingDirection.Center;

    private float lastBlinkTime = -10f;

    private bool commandsEnabled = false;

    private void Awake()
    {
        if (doorSelectionManager == null)
        {
            Debug.LogError("DoorSelectionManager reference missing.");
            enabled = false;
            // return;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            ToggleCommands();
        }

        if (!commandsEnabled)
        {
            return;
        }

        EyeTrackingDirection directions = UFeelAPI.EyeTracking.Directions;
        bool? blinkStatus = UFeelAPI.EyeTracking.IsBlinking;

        ProcessInput(directions, blinkStatus == true);
    }

    private void ToggleCommands()
    {
        commandsEnabled = !commandsEnabled;
    }

    private void ProcessInput(EyeTrackingDirection directions, bool blink)
    {
        bool blinkPressed = blink && !previousBlink;
        previousBlink = blink;
        bool doubleBlink = DetectDoubleBlink(blinkPressed);

        (bool leftGesture, bool rightGesture) = ProcessGaze(directions);

        if (leftGesture)
        {
            doorSelectionManager.SelectPreviousDoor();
        }

        if (rightGesture)
        {
            doorSelectionManager.SelectNextDoor();
        }

        if (doubleBlink)
        {
            doorSelectionManager.ConfirmCurrentSelection();
        }
    }

    private static EyeTrackingDirection GetEyeTrackingDirection(EyeTrackingDirection directions)
    {
        if (directions.HasDirection(EyeTrackingDirection.Left) && !directions.HasDirection(EyeTrackingDirection.Right))
            return EyeTrackingDirection.Left;

        if (directions.HasDirection(EyeTrackingDirection.Right) && !directions.HasDirection(EyeTrackingDirection.Left))
            return EyeTrackingDirection.Right;

        return EyeTrackingDirection.Center;
    }

    private (bool Left, bool Right) ProcessGaze(EyeTrackingDirection directions)
    {
        EyeTrackingDirection currentDirection = GetEyeTrackingDirection(directions);

        bool leftGesture =
            previousEyeTrackingDirection.HasDirection(EyeTrackingDirection.Left) &&
            currentDirection.HasDirection(EyeTrackingDirection.Center);

        bool rightGesture =
            previousEyeTrackingDirection.HasDirection(EyeTrackingDirection.Right) &&
            currentDirection.HasDirection(EyeTrackingDirection.Center);

        previousEyeTrackingDirection = currentDirection;

        return (leftGesture, rightGesture);
    }

    private bool DetectDoubleBlink(bool blinkPressed)
    {
        if (!blinkPressed)
            return false;

        if (Time.time - lastBlinkTime <= doubleBlinkWindow)
        {
            lastBlinkTime = -10f;
            return true;
        }

        lastBlinkTime = Time.time;
        return false;
    }
}
