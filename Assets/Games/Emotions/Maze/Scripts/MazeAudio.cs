using GameHub.Common;
using UnityEngine;

namespace GameHub.Maze
{
    public sealed class MazeAudio
    {
        private const float DoorVolume = 0.9f;
        private const float CueVolume = 0.7f;
        private const float MinDistance = 2f;
        private const float MaxDistance = 25f;
        private const int SpatialVoices = 3;

        private readonly MazeSoundSet sounds;
        private readonly GameObject root;
        private readonly AudioSource cues;
        private readonly ChargeSound charge;
        private readonly AudioSource[] voices = new AudioSource[SpatialVoices];
        private int nextVoice;

        public MazeAudio(Transform owner, MazeSoundSet sounds)
        {
            this.sounds = sounds;
            root = new GameObject("MazeAudio");
            root.transform.SetParent(owner, false);

            cues = CreateSource("Cues", AudioChannel.Effects, false);
            charge = new ChargeSound(CreateSource("Charge", AudioChannel.Effects, true));
            for (int i = 0; i < SpatialVoices; i++)
                voices[i] = CreateSource($"Voice{i}", AudioChannel.Effects, true);
        }

        public void Tick(float deltaTime)
        {
            charge.Tick(deltaTime);
        }

        public void Charge(Vector3 position, float progress)
        {
            charge.Source.transform.position = position;
            charge.Set(progress);
        }

        public void StopCharge()
        {
            charge.Stop();
        }

        public void DoorOpened(Vector3 position) => PlayAt(sounds.DoorOpen, position, DoorVolume);

        public void DoorClosed(Vector3 position) => PlayAt(sounds.DoorClose, position, DoorVolume);

        public void Hint() => cues.PlayOneShot(sounds.Hint, CueVolume);

        public void Bonus() => cues.PlayOneShot(sounds.Bonus, CueVolume);

        public void Trap() => cues.PlayOneShot(sounds.Trap, CueVolume);

        public void Win() => cues.PlayOneShot(sounds.Win, CueVolume);

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
