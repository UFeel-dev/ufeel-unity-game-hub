using UFeel;
using UnityEngine;

public class DoorEyeController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DoorSelectionManager doorSelectionManager;

    [Header("Settings")]
    [SerializeField] private float doubleBlinkWindow = 0.5f;

    private bool previousBlink;

    private GazeDirection previousGazeDirection = GazeDirection.Center;

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

        // TODO: Enable/Disable method for Gaze detection
        // UFeelAPI.Gaze.();
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

        GazeDirection directions = UFeelAPI.Gaze.CurrentDirections;
        bool? blinkStatus = UFeelAPI.Gaze.IsBlinking;

        ProcessInput(directions, blinkStatus == true);
    }

    private void ToggleCommands()
    {
        commandsEnabled = !commandsEnabled;
    }

    private void ProcessInput(GazeDirection directions, bool blink)
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

    private static GazeDirection GetGazeDirection(GazeDirection directions)
    {
        if (directions.IsDirection(GazeDirection.Left) && !directions.IsDirection(GazeDirection.Right))
            return GazeDirection.Left;

        if (directions.IsDirection(GazeDirection.Right) && !directions.IsDirection(GazeDirection.Left))
            return GazeDirection.Right;

        return GazeDirection.Center;
    }

    private (bool Left, bool Right) ProcessGaze(GazeDirection directions)
    {
        GazeDirection currentDirection = GetGazeDirection(directions);

        bool leftGesture =
            previousGazeDirection.IsDirection(GazeDirection.Left) &&
            currentDirection.IsDirection(GazeDirection.Center);

        bool rightGesture =
            previousGazeDirection.IsDirection(GazeDirection.Right) &&
            currentDirection.IsDirection(GazeDirection.Center);

        previousGazeDirection = currentDirection;

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
