using UnityEngine;
using UFeel;

public class BlinkDetector : MonoBehaviour
{
    public static BlinkDetector Instance { get; private set; }

    [SerializeField] private float doubleBlinkWindow = 0.35f;

    public bool BlinkPressed { get; private set; }
    public bool DoubleBlinkPressed { get; private set; }

    private bool previousBlink;
    private float lastBlinkTime;

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
        BlinkPressed = false;
        DoubleBlinkPressed = false;

        bool blink = UFeelAPI.Gaze.IsBlinking;

        bool blinkEdge = blink && !previousBlink;

        if (blinkEdge)
        {
            float timeSinceLast = Time.time - lastBlinkTime;

            if (timeSinceLast <= doubleBlinkWindow)
            {
                DoubleBlinkPressed = true;
                lastBlinkTime = 0f;
            }
            else
            {
                BlinkPressed = true;
                lastBlinkTime = Time.time;
            }
        }

        previousBlink = blink;
    }
}
