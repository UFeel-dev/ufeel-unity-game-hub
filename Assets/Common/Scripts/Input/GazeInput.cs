using UFeel;

namespace GameHub.Common
{
    public static class GazeInput
    {
        public static int HorizontalDirection()
        {
            EyeTrackingDirection directions = UFeelAPI.EyeTracking.Directions;
            return Axis(directions.HasDirection(EyeTrackingDirection.Left), directions.HasDirection(EyeTrackingDirection.Right));
        }

        public static int VerticalDirection()
        {
            EyeTrackingDirection directions = UFeelAPI.EyeTracking.Directions;
            return Axis(directions.HasDirection(EyeTrackingDirection.Down), directions.HasDirection(EyeTrackingDirection.Up));
        }

        private static int Axis(bool negative, bool positive)
        {
            if (negative == positive)
                return 0;

            return negative ? -1 : 1;
        }
    }
}
