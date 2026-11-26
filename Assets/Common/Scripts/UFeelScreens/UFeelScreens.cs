using UFeel;
using UnityEngine;

namespace GameHub.Common
{
    public sealed class UFeelScreens : MonoBehaviour
    {
        private const KeyCode CameraKey = KeyCode.F1;
        private const KeyCode CalibrationKey = KeyCode.F8;
        private const KeyCode MicrophoneKey = KeyCode.F9;

        private const float ToastOffset = 40f;

        private static UFeelScreens? instance;

        private readonly PointerInput mouse = new();

        private CameraPanel cameraPanel;
        private MicrophonePanel microphone;
        private CalibrationScreen calibration;
        private Toast toast;

        public static void ShowMicrophone()
        {
            if (instance != null)
                instance.microphone.Show();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance == null)
                new GameObject(nameof(UFeelScreens)).AddComponent<UFeelScreens>();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            UiKit kit = UiKit.Shared;
            RectTransform canvas = UiCanvas.Create("UFeelScreens", transform, UiCanvas.SystemOrder);
            cameraPanel = new CameraPanel(kit, canvas);
            microphone = new MicrophonePanel(kit, canvas);
            toast = new Toast(kit, canvas, new Vector2(0.5f, 1f), ToastOffset);
            calibration = new CalibrationScreen(kit, canvas, toast);
        }

        private void OnEnable()
        {
            UFeelAPI.SpeechToText.MicrophonesChanged += OnMicrophonesChanged;
        }

        private void OnDisable()
        {
            UFeelAPI.SpeechToText.MicrophonesChanged -= OnMicrophonesChanged;
        }

        private void OnDestroy()
        {
            if (instance != this)
                return;

            microphone.Hide();
            UFeelAPI.CameraFeed.IsCapturing = false;
            instance = null;
        }

        private void Update()
        {
            float deltaTime = Time.unscaledDeltaTime;
            mouse.Tick();

            if (LayoutInput.GetKeyDown(CameraKey))
                cameraPanel.Toggle();
            if (!UFeelAPI.EyeTracking.IsCalibrating && LayoutInput.GetKeyDown(CalibrationKey))
                calibration.Begin();
            else if (!UFeelAPI.EyeTracking.IsCalibrating && LayoutInput.GetKeyDown(MicrophoneKey))
                microphone.Toggle();

            cameraPanel.Tick();
            microphone.Tick(deltaTime, mouse);
            calibration.Tick(deltaTime);
            toast.Tick(deltaTime);
        }

        private void OnMicrophonesChanged()
        {
            microphone.OnMicrophonesChanged();
        }
    }
}
