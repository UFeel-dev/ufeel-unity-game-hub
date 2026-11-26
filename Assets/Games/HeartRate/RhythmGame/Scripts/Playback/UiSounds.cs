using UnityEngine;

namespace GameHub.RhythmGame
{
    public sealed class UiSounds
    {
        private readonly AudioSource source;
        private readonly AudioClip? beep;
        private readonly AudioClip flatline;

        public UiSounds(AudioSource source, AudioClip? beep, AudioClip flatline)
        {
            this.source = source;
            this.beep = beep;
            this.flatline = flatline;
        }

        public void Move()
        {
            PlayBeep(1.8f, 0.05f);
        }

        public void Confirm()
        {
            PlayBeep(1.3f, 0.12f);
        }

        public void Back()
        {
            PlayBeep(0.9f, 0.08f);
        }

        public void Flatline()
        {
            source.pitch = 1f;
            source.PlayOneShot(flatline, 0.6f);
        }

        private void PlayBeep(float pitch, float volume)
        {
            if (beep == null)
                return;

            source.pitch = pitch;
            source.PlayOneShot(beep, volume);
        }
    }
}
