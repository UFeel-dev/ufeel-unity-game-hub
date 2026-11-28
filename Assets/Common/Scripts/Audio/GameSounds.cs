using System;
using UnityEngine;

namespace GameHub.Common
{
    public static class GameSounds
    {
        private const string Folder = "Audio/Cues/";
        private const float MenuVolume = 0.28f;
        private const float CueVolume = 0.45f;

        private static readonly AudioClip?[] Clips = new AudioClip?[Enum.GetValues(typeof(SoundCue)).Length];
        private static readonly bool[] Loaded = new bool[Clips.Length];

        private static AudioSource? source;

        public static void Play(SoundCue cue, float volume = 1f)
        {
            AudioClip? clip = Clip(cue);
            if (clip != null)
                Source().PlayOneShot(clip, volume * (cue is SoundCue.MenuMove or SoundCue.MenuConfirm or SoundCue.MenuBack ? MenuVolume : CueVolume));
        }

        public static AudioClip? Clip(SoundCue cue)
        {
            int index = (int)cue;
            if (!Loaded[index])
            {
                Clips[index] = Resources.Load<AudioClip>(Folder + cue);
                Loaded[index] = true;
            }

            return Clips[index];
        }

        private static AudioSource Source()
        {
            if (source != null)
                return source;

            GameObject holder = new(nameof(GameSounds));
            UnityEngine.Object.DontDestroyOnLoad(holder);
            source = holder.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.ignoreListenerPause = true;
            GameAudio.Route(source, AudioChannel.Effects);
            return source;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            Array.Clear(Clips, 0, Clips.Length);
            Array.Clear(Loaded, 0, Loaded.Length);
            source = null;
        }
    }
}
