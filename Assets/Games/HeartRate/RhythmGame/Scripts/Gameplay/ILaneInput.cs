namespace GameHub.RhythmGame
{
    public interface ILaneInput
    {
        public bool IsHeld(int lane);

        public bool WasPressed(int lane);

        public bool WasReleased(int lane);
    }
}
