using UFeel;
using UnityEngine;

namespace GameHub.Survivor
{
    public class EyeInput : MonoBehaviour
    {
        public static EyeInput? Instance { get; private set; }

        public bool LookLeft { get; private set; }

        public bool LookRight { get; private set; }

        public bool LeftPressed { get; private set; }

        public bool RightPressed { get; private set; }

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
            EyeTrackingDirection directions = UFeelAPI.EyeTracking.Directions;
            bool left = directions.HasDirection(EyeTrackingDirection.Left);
            bool right = directions.HasDirection(EyeTrackingDirection.Right);

            LeftPressed = left && !LookLeft;
            RightPressed = right && !LookRight;
            LookLeft = left;
            LookRight = right;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
