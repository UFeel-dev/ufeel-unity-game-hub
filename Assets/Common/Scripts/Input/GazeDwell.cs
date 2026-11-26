using UnityEngine;

namespace GameHub.Common
{
    public sealed class GazeDwell
    {
        private const float DwellSeconds = 0.4f;
        private const float RepeatSeconds = 1f;

        private float lookTime;
        private float lookTarget = DwellSeconds;

        public int Direction { get; private set; }

        public float Progress => Direction == 0 ? 0f : Mathf.Clamp01(lookTime / lookTarget);

        public int Tick(int direction, float deltaTime)
        {
            if (direction != Direction)
            {
                Direction = direction;
                lookTime = 0f;
                lookTarget = DwellSeconds;
            }

            if (Direction == 0)
                return 0;

            lookTime += deltaTime;
            if (lookTime < lookTarget)
                return 0;

            lookTime = 0f;
            lookTarget = RepeatSeconds;
            return Direction;
        }

        public void Reset()
        {
            Direction = 0;
            lookTime = 0f;
            lookTarget = DwellSeconds;
        }
    }
}
