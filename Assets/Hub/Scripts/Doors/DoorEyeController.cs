using GameHub.Common;
using UFeel;
using UnityEngine;

namespace GameHub.Hub
{
    public class DoorEyeController : MonoBehaviour
    {
        [SerializeField] private DoorSelectionManager doorSelectionManager;
        [SerializeField] private float doubleBlinkWindow = 0.5f;

        private bool commandsEnabled;
        private bool previousBlink;
        private int previousDirection;
        private float lastBlinkTime = float.NegativeInfinity;

        private void Awake()
        {
            if (doorSelectionManager != null)
                return;

            Debug.LogError("DoorSelectionManager reference missing.", this);
            enabled = false;
        }

        private void Update()
        {
            if (UiFocus.IsFree && LayoutInput.GetKeyDown(KeyCode.N))
            {
                commandsEnabled = !commandsEnabled;
                doorSelectionManager.SetEyeControl(commandsEnabled);
            }

            if (!commandsEnabled)
                return;

            int direction = GazeInput.HorizontalDirection();
            if (direction == 0 && previousDirection < 0)
                doorSelectionManager.SelectPrevious();
            else if (direction == 0 && previousDirection > 0)
                doorSelectionManager.SelectNext();

            previousDirection = direction;

            if (DetectDoubleBlink())
                doorSelectionManager.ConfirmSelection();
        }

        private bool DetectDoubleBlink()
        {
            bool blink = UFeelAPI.EyeTracking.IsBlinking;
            bool blinkStarted = blink && !previousBlink;
            previousBlink = blink;

            if (!blinkStarted)
                return false;

            if (Time.time - lastBlinkTime <= doubleBlinkWindow)
            {
                lastBlinkTime = float.NegativeInfinity;
                return true;
            }

            lastBlinkTime = Time.time;
            return false;
        }
    }
}
