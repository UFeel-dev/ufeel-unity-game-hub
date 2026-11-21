#nullable enable

using System;
using UnityEngine;

namespace UFeel
{
    [DefaultExecutionOrder(-1000)]
    public sealed partial class UFeelAPI : MonoBehaviour
    {
        private static readonly RuleSet Rules = new RuleSet { ErrorHandler = Debug.LogException };

        private static UFeelAPI? instance;
        private static UFeelSession? session;
        private static CalibrationProfileStore? profiles;
        private static UFeelSnapshot frameSnapshot = UFeelSnapshot.Initial;
        private static int frameSnapshotFrame = -1;

        private UFeelCalibrationOverlay? calibrationOverlay;

        /// <summary>Null when the native library failed to start.</summary>
        public static UFeelSession? Session => session;

        public static bool IsAvailable => session != null;

        public static RuleKey AddRule(Func<bool> condition, Action action, bool onlyOnce)
        {
            return Rules.Add(condition, action, onlyOnce);
        }

        public static void RemoveRule(RuleKey key) => Rules.Remove(key);

        /// <summary>Shared by every read in the current frame, so values stay consistent.</summary>
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
            calibrationOverlay = gameObject.AddComponent<UFeelCalibrationOverlay>();

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

                session = new UFeelSession(processor);
            }
            catch
            {
                processor.Dispose();
                throw;
            }

            session.Start();
        }

        private void Update() => Rules.Evaluate();

        private void OnApplicationQuit() => Shutdown();

        private void OnDestroy()
        {
            if (instance != this)
                return;

            Shutdown();
            instance = null;
        }

        private static void Shutdown()
        {
            UFeelSession? running = session;
            session = null;
            frameSnapshotFrame = -1;
            Rules.Clear();

            running?.Dispose();
        }
    }
}
