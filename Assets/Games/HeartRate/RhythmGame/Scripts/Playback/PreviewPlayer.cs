using UnityEngine;

namespace GameHub.RhythmGame
{
    public sealed class PreviewPlayer
    {
        private const float Volume = 0.6f;
        private const float FadeOutSeconds = 0.25f;
        private const float FadeInSeconds = 0.6f;
        private const float WindowSeconds = 20f;
        private const float EndMargin = 0.5f;

        private readonly AudioSource source;

        private RhythmSong? requested;
        private RhythmSong? playing;
        private float windowEnd;
        private float volume;

        public PreviewPlayer(AudioSource source)
        {
            this.source = source;
        }

        public void Play(RhythmSong? song)
        {
            requested = song;
        }

        public void Stop()
        {
            requested = null;
        }

        public void Tick(float deltaTime)
        {
            if (playing != null && NeedsRestart())
            {
                volume = Mathf.MoveTowards(volume, 0f, deltaTime * Volume / FadeOutSeconds);
                source.volume = volume;
                if (volume > 0f)
                    return;

                source.Stop();
                playing = null;
            }

            if (playing == null && requested != null)
                TryStart(requested);

            if (playing != null)
            {
                volume = Mathf.MoveTowards(volume, Volume, deltaTime * Volume / FadeInSeconds);
                source.volume = volume;
            }
        }

        private bool NeedsRestart()
        {
            return playing != requested || !source.isPlaying || source.time >= windowEnd;
        }

        private void TryStart(RhythmSong song)
        {
            AudioClip clip = song.Clip;
            if (clip == null)
                return;
            if (clip.loadState == AudioDataLoadState.Unloaded)
                clip.LoadAudioData();
            if (clip.loadState != AudioDataLoadState.Loaded)
                return;

            float start = Mathf.Clamp(song.PreviewStart, 0f, Mathf.Max(0f, clip.length - WindowSeconds));
            source.clip = clip;
            source.pitch = 1f;
            source.time = start;
            windowEnd = Mathf.Min(start + WindowSeconds, clip.length - EndMargin);
            volume = 0f;
            source.volume = 0f;
            source.Play();
            playing = song;
        }
    }
}
