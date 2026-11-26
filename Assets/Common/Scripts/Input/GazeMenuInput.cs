using UFeel;
using UnityEngine;

namespace GameHub.Common
{
    public sealed class GazeMenuInput
    {
        private const float CloseSeconds = 0.7f;

        private readonly GazeDwell look = new();

        private float closedTime;
        private bool confirmed;

        public int Direction => look.Direction;

        public float StepProgress => look.Progress;

        public float ConfirmProgress => confirmed ? 0f : Mathf.Clamp01(closedTime / CloseSeconds);

        public GazeAction Tick(float deltaTime)
        {
            if (UFeelAPI.EyeTracking.IsBlinking)
                return TickClosed(deltaTime);

            closedTime = 0f;
            confirmed = false;
            return look.Tick(GazeInput.HorizontalDirection(), deltaTime) switch
            {
                < 0 => GazeAction.Previous,
                > 0 => GazeAction.Next,
                _ => GazeAction.None,
            };
        }

        public void Reset()
        {
            look.Reset();
            closedTime = 0f;
            confirmed = false;
        }

        private GazeAction TickClosed(float deltaTime)
        {
            if (confirmed)
                return GazeAction.None;

            closedTime += deltaTime;
            if (closedTime < CloseSeconds)
                return GazeAction.None;

            confirmed = true;
            look.Reset();
            return GazeAction.Confirm;
        }
    }
}
