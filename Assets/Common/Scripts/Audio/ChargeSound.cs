using UnityEngine;

namespace GameHub.Common
{
    public sealed class ChargeSound
    {
        private const float Volume = 0.45f;
        private const float MinPitch = 0.8f;
        private const float PitchRange = 0.6f;
        private const float Fade = 4f;

        private float target;

        public ChargeSound(AudioSource source)
        {
            Source = source;
            source.clip = GameSounds.Clip(SoundCue.Charge);
            source.loop = true;
            source.volume = 0f;
        }

        public AudioSource Source { get; }

        public void Set(float progress)
        {
            Source.pitch = MinPitch + (progress * PitchRange);
            target = progress > 0f ? Volume * (0.3f + (0.7f * progress)) : 0f;
            if (target > 0f && !Source.isPlaying)
                Source.Play();
        }

        public void Stop()
        {
            target = 0f;
        }

        public void Tick(float deltaTime)
        {
            Source.volume = Mathf.MoveTowards(Source.volume, target, Fade * deltaTime);
            if (Source.volume <= 0f && Source.isPlaying)
                Source.Stop();
        }
    }
}
