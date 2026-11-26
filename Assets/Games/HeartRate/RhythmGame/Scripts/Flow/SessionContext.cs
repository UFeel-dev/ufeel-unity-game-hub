using UnityEngine;

namespace GameHub.RhythmGame
{
    public sealed class SessionContext
    {
        public SessionContext(StageView stage, AudioSource music, HeartRateMonitor monitor, UiSounds sounds)
        {
            Stage = stage;
            Music = music;
            Conductor = new SongConductor(music);
            Monitor = monitor;
            Sounds = sounds;
        }

        public StageView Stage { get; }

        public AudioSource Music { get; }

        public SongConductor Conductor { get; }

        public HeartRateMonitor Monitor { get; }

        public UiSounds Sounds { get; }

        public RestingRate Resting { get; } = new();

        public PointerInput Mouse { get; } = new();
    }
}
