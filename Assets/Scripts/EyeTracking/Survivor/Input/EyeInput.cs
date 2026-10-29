using UFeel;
using UnityEngine;

public class EyeInput : MonoBehaviour
{
    public static EyeInput Instance { get; private set; }

    public bool LookLeft { get; private set; }
    public bool LookRight { get; private set; }

    public bool LeftPressed { get; private set; }
    public bool RightPressed { get; private set; }

    private bool previousLeft;
    private bool previousRight;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        GazeDirection data = UFeelAPI.Gaze.CurrentDirections;

        bool left = data.IsDirection(GazeDirection.Left);
        bool right = data.IsDirection(GazeDirection.Right);

        LookLeft = left;
        LookRight = right;

        LeftPressed = left && !previousLeft;
        RightPressed = right && !previousRight;

        previousLeft = left;
        previousRight = right;
    }
}
