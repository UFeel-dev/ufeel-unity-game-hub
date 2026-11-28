using UnityEngine;

namespace GameHub.Common
{
    public sealed class CalibrationOverlay
    {
        private readonly GameObject root;

        private float pausedTimeScale = 1f;

        public CalibrationOverlay(GameObject root)
        {
            this.root = root;
            root.SetActive(false);
        }

        public bool IsShown { get; private set; }

        public void SetShown(bool shown)
        {
            if (shown == IsShown)
                return;

            IsShown = shown;
            root.SetActive(shown);
            if (shown)
            {
                pausedTimeScale = Time.timeScale;
                Time.timeScale = 0f;
                UiFocus.Capture(this);
            }
            else
            {
                Time.timeScale = pausedTimeScale;
                UiFocus.Release(this);
            }
        }
    }
}
