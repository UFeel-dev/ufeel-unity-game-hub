using UFeel;
using UnityEngine;

namespace GameHub.Common
{
    public sealed class UFeelScreens : MonoBehaviour
    {
        private const float ToastOffset = 40f;

        private static UFeelScreens? instance;

        private readonly PointerInput mouse = new();

        private CameraPanel cameraPanel;
        private MicrophonePanel microphone;
        private EyeTrackingCalibrationScreen eyeTrackingCalibration;
        private EmotionCalibrationScreen emotionCalibration;
        private CalibrationToast toast;

        private static bool IsCalibrating => UFeelAPI.EyeTracking.IsCalibrating || UFeelAPI.Emotion.IsCalibrating;

        public static void ShowMicrophone()
        {
            if (instance != null)
                instance.microphone.Show();
        }

        public static void BeginEyeTrackingCalibration()
        {
            if (instance != null && !IsCalibrating)
                instance.eyeTrackingCalibration.Begin();
        }

        public static void BeginEmotionCalibration()
        {
            if (instance != null && !IsCalibrating)
                instance.emotionCalibration.Begin();
        }

        public static void ResetEmotionCalibration()
        {
            if (instance != null)
                instance.emotionCalibration.Reset();
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
            toast = new CalibrationToast(new Toast(kit, canvas, new Vector2(0.5f, 1f), ToastOffset));
            eyeTrackingCalibration = new EyeTrackingCalibrationScreen(kit, canvas, toast);
            emotionCalibration = new EmotionCalibrationScreen(kit, canvas, toast);
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

            if (KeyBinds.Down(KeyAction.CameraPanel))
                cameraPanel.Toggle();
            if (KeyBinds.Down(KeyAction.EmotionCalibration))
                BeginEmotionCalibration();
            else if (KeyBinds.Down(KeyAction.EyeTrackingCalibration))
                BeginEyeTrackingCalibration();
            else if (!IsCalibrating && KeyBinds.Down(KeyAction.Microphone))
                microphone.Toggle();

            UFeelAPI.CameraFeed.IsCapturing = cameraPanel.IsVisible || emotionCalibration.IsShown;
            cameraPanel.Tick();
            microphone.Tick(deltaTime, mouse);
            eyeTrackingCalibration.Tick(deltaTime);
            emotionCalibration.Tick();
            toast.Tick(deltaTime, eyeTrackingCalibration.Hint);
        }

        private void OnMicrophonesChanged()
        {
            microphone.OnMicrophonesChanged();
        }
    }
}
