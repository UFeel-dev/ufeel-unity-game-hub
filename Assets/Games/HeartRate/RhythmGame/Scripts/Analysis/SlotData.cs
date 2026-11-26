namespace GameHub.RhythmGame
{
    public sealed class SlotData
    {
        public readonly float[] Strength;
        public readonly OnsetBand[] Bands;
        public readonly float[] Intensity;
        public readonly int[] Sustain;

        public SlotData(int count)
        {
            Strength = new float[count];
            Bands = new OnsetBand[count];
            Intensity = new float[count];
            Sustain = new int[count];
        }

        public int Count => Strength.Length;
    }
}
