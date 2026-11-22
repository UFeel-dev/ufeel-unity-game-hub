using UFeel;

namespace GameHub.Common
{
    public static class GazeInput
    {
        public static int HorizontalDirection()
        {
            EyeTrackingDirection directions = UFeelAPI.EyeTracking.Directions;
            bool left = directions.HasDirection(EyeTrackingDirection.Left);
            bool right = directions.HasDirection(EyeTrackingDirection.Right);

            if (left == right)
                return 0;

            return left ? -1 : 1;
        }
    }
}
