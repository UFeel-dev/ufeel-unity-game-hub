using GameHub.Common;
using UnityEngine;

namespace GameHub.Maze
{
    public sealed class MazeAudio
    {
        private const float AmbientVolume = 0.25f;
        private const float ChargeVolume = 0.45f;
        private const float ChargeMinPitch = 0.8f;
        private const float ChargePitchRange = 0.6f;
        private const float ChargeFade = 4f;
        private const float DoorVolume = 0.9f;
        private const float CueVolume = 0.7f;
        private const float StepVolume = 0.35f;
        private const float StepDistance = 2f;
        private const float StepPitchRange = 0.12f;
        private const float TeleportDistance = 5f;
        private const float MinDistance = 2f;
        private const float MaxDistance = 25f;
        private const int SpatialVoices = 3;

        private readonly MazeSounds sounds;
        private readonly GameObject root;
        private readonly AudioSource ambient;
        private readonly AudioSource cues;
        private readonly AudioSource steps;
        private readonly AudioSource charge;
        private readonly AudioSource[] voices = new AudioSource[SpatialVoices];
        private int nextVoice;
        private float chargeTarget;
        private Vector3 lastStep;
        private float walked;

        public MazeAudio(Transform owner, MazeSounds sounds)
        {
            this.sounds = sounds;
            root = new GameObject("MazeAudio");
            root.transform.SetParent(owner, false);

            ambient = CreateSource("Ambient", AudioChannel.Music, false);
            ambient.clip = sounds.Ambient;
            ambient.loop = true;
            ambient.volume = AmbientVolume;
            ambient.Play();

            cues = CreateSource("Cues", AudioChannel.Effects, false);
            steps = CreateSource("Steps", AudioChannel.Effects, false);
            charge = CreateSource("Charge", AudioChannel.Effects, true);
            charge.clip = sounds.Charge;
            charge.loop = true;
            charge.volume = 0f;
            for (int i = 0; i < SpatialVoices; i++)
                voices[i] = CreateSource($"Voice{i}", AudioChannel.Effects, true);
        }

        public void Tick(float deltaTime)
        {
            charge.volume = Mathf.MoveTowards(charge.volume, chargeTarget, ChargeFade * deltaTime);
            if (charge.volume <= 0f && charge.isPlaying)
                charge.Stop();
        }

        public void Charge(Vector3 position, float progress)
        {
            charge.transform.position = position;
            charge.pitch = ChargeMinPitch + (progress * ChargePitchRange);
            chargeTarget = progress > 0f ? ChargeVolume * (0.3f + (0.7f * progress)) : 0f;
            if (chargeTarget > 0f && !charge.isPlaying)
                charge.Play();
        }

        public void StopCharge()
        {
            chargeTarget = 0f;
        }

        public void DoorOpened(Vector3 position) => PlayAt(sounds.DoorOpen, position, DoorVolume);

        public void DoorClosed(Vector3 position) => PlayAt(sounds.DoorClose, position, DoorVolume);

        public void Hint() => cues.PlayOneShot(sounds.Hint, CueVolume);

        public void Bonus() => cues.PlayOneShot(sounds.Bonus, CueVolume);

        public void Trap() => cues.PlayOneShot(sounds.Trap, CueVolume);

        public void Win() => cues.PlayOneShot(sounds.Win, CueVolume);

        public void Walk(Vector3 position)
        {
            Vector3 step = position - lastStep;
            step.y = 0f;
            lastStep = position;
            float distance = step.magnitude;
            if (distance > TeleportDistance)
                return;

            walked += distance;
            if (walked < StepDistance)
                return;

            walked = 0f;
            steps.pitch = 1f + Random.Range(-StepPitchRange, StepPitchRange);
            steps.PlayOneShot(sounds.Steps[Random.Range(0, sounds.Steps.Count)], StepVolume);
        }

        private void PlayAt(AudioClip clip, Vector3 position, float volume)
        {
            AudioSource voice = voices[nextVoice];
            nextVoice = (nextVoice + 1) % voices.Length;
            voice.transform.position = position;
            voice.PlayOneShot(clip, volume);
        }

        private AudioSource CreateSource(string name, AudioChannel channel, bool spatial)
        {
            GameObject holder = new(name);
            holder.transform.SetParent(root.transform, false);
            AudioSource source = holder.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = spatial ? 1f : 0f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = MinDistance;
            source.maxDistance = MaxDistance;
            source.dopplerLevel = 0f;
            GameAudio.Route(source, channel);
            return source;
        }
    }
}
