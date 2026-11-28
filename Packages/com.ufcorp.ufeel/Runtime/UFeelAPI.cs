#nullable enable

using System;
using UnityEngine;

namespace UFeel
{
    [DefaultExecutionOrder(-1000)]
    public sealed partial class UFeelAPI : MonoBehaviour
    {
        private static readonly RuleSet Rules = new() { ErrorHandler = Debug.LogException };
        private static readonly UFeelFrameCapture FrameCapture = new();

        private static UFeelAPI? instance;
        private static UFeelSession? session;
        private static CalibrationProfileStore? profiles;
        private static UFeelSnapshot frameSnapshot = UFeelSnapshot.Initial;
        private static int frameSnapshotFrame = -1;

        /// <summary>Gets the time of the last read of any feature, or 0 if none was read yet.</summary>
        /// <remarks>Null when the native library failed to start.</remarks>
        public static UFeelSession? Session => session;

        public static bool IsAvailable => session != null;

        public static RuleKey AddRule(Func<bool> condition, Action action, bool onlyOnce)
        {
            return Rules.Add(condition, action, onlyOnce);
        }

        public static void RemoveRule(RuleKey key) => Rules.Remove(key);

        internal static UFeelSnapshot Snapshot
        {
            get
            {
                int frame = Time.frameCount;
                if (frame != frameSnapshotFrame)
                {
                    frameSnapshot = session?.Latest ?? UFeelSnapshot.Initial;
                    frameSnapshotFrame = frame;
                }

                return frameSnapshot;
            }
        }

        /// <summary>Also keeps <paramref name="feature"/> running.</summary>
        /// <param name="feature">The feature to read.</param>
        /// <returns>The latest snapshot of the requested feature.</returns>
        internal static UFeelSnapshot Read(DetectionType feature)
        {
            session?.Demand.Request(feature);
            return Snapshot;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (instance == null)
                new GameObject(nameof(UFeelAPI)).AddComponent<UFeelAPI>();
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
            Begin();
        }

        private void OnEnable()
        {
            if (instance != null)
                return;

            instance = this;
            Begin();
        }

        private static void Begin()
        {
#if UNITY_EDITOR
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += Shutdown;
#endif
            try
            {
                StartSession();
            }
            catch (Exception exception)
            {
                Debug.LogError($"UFeel is unavailable: {exception.Message}");
                Debug.LogException(exception);
            }
        }

        private static void StartSession()
        {
            UFeelProcessor processor = new(UFeelPackageLayout.FromRoot(UFeelPackagePaths.NativeRoot));
            try
            {
                profiles = CalibrationProfileStore.CreateDefault();
                ProfileLoadResult profile = profiles.Load(processor);
                EyeTracking.Calibrated = profile == ProfileLoadResult.Loaded;
                processor.SetEyeTrackingHeadWeight(EyeTracking.HeadWeight);
                Debug.Log($"UFeel started (eye-tracking profile: {profile}, {profiles.Path})");

                session = new UFeelSession(processor, frameObserver: FrameCapture);
            }
            catch
            {
                processor.Dispose();
                throw;
            }

            SpeechToText.WatchMicrophones(session);
            session.Start();
            SpeechToText.RestoreMicrophone(session);
        }

        private void Update()
        {
            SpeechToText.RaiseMicrophonesChanged();
            CameraFeed.Upload();
            Rules.Evaluate();
        }

        private void OnApplicationQuit() => Shutdown();

        private void OnDestroy()
        {
            if (instance != this)
                return;

#if UNITY_EDITOR
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload -= Shutdown;
#endif
            Shutdown();
            instance = null;
        }

        private static void Shutdown()
        {
            EyeTracking.CancelCalibration();
            UFeelSession? running = session;
            session = null;
            frameSnapshotFrame = -1;
            Rules.Clear();
            CameraFeed.Release();

            running?.Dispose();
        }
    }
}
