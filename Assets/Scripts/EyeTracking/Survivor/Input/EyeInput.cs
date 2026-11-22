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
        EyeTrackingDirection data = UFeelAPI.EyeTracking.Directions;

        bool left = data.HasDirection(EyeTrackingDirection.Left);
        bool right = data.HasDirection(EyeTrackingDirection.Right);

        LookLeft = left;
        LookRight = right;

        LeftPressed = left && !previousLeft;
        RightPressed = right && !previousRight;

        previousLeft = left;
        previousRight = right;
    }
}
