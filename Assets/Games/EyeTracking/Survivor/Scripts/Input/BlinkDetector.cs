using UFeel;
using UnityEngine;

namespace GameHub.Survivor
{
    public class BlinkDetector : MonoBehaviour
    {
        [SerializeField] private float doubleBlinkWindow = 0.35f;

        private bool previousBlink;
        private float lastBlinkTime = float.NegativeInfinity;

        public static BlinkDetector? Instance { get; private set; }

        public bool BlinkPressed { get; private set; }

        public bool DoubleBlinkPressed { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Update()
        {
            bool blink = UFeelAPI.EyeTracking.IsBlinking;
            bool blinkStarted = blink && !previousBlink;
            previousBlink = blink;

            BlinkPressed = false;
            DoubleBlinkPressed = false;

            if (!blinkStarted)
                return;

            if (Time.time - lastBlinkTime <= doubleBlinkWindow)
            {
                DoubleBlinkPressed = true;
                lastBlinkTime = float.NegativeInfinity;
            }
            else
            {
                BlinkPressed = true;
                lastBlinkTime = Time.time;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
