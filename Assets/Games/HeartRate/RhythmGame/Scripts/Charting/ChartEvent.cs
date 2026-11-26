namespace GameHub.RhythmGame
{
    public readonly struct ChartEvent
    {
        public ChartEvent(int slot, int size, int holdEnd)
        {
            Slot = slot;
            Size = size;
            HoldEnd = holdEnd;
        }

        public int Slot { get; }

        public int Size { get; }

        public int HoldEnd { get; }

        public bool IsHold => HoldEnd > Slot;

        public ChartEvent WithSize(int size)
        {
            return new ChartEvent(Slot, size, HoldEnd);
        }

        public ChartEvent WithHold(int holdEnd)
        {
            return new ChartEvent(Slot, Size, holdEnd);
        }

        public ChartEvent Shifted(int slots)
        {
            return new ChartEvent(Slot + slots, Size, HoldEnd + slots);
        }
    }
}
