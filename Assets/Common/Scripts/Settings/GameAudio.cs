using UnityEngine;
using UnityEngine.Audio;

namespace GameHub.Common
{
    public static class GameAudio
    {
        private const string MixerResource = "Audio/GameMixer";
        private const string MusicParameter = "MusicVolume";
        private const string EffectsParameter = "EffectsVolume";
        private const float SilentDecibels = -80f;

        private static AudioMixer? mixer;
        private static AudioMixerGroup? musicGroup;
        private static AudioMixerGroup? effectsGroup;

        public static void Route(AudioSource source, AudioChannel channel)
        {
            Load();
            source.outputAudioMixerGroup = channel == AudioChannel.Music ? musicGroup : effectsGroup;
        }

        public static void SetVolumes(float master, float music, float effects)
        {
            AudioListener.volume = master;
            Load();
            mixer!.SetFloat(MusicParameter, Decibels(music));
            mixer.SetFloat(EffectsParameter, Decibels(effects));
        }

        private static float Decibels(float volume)
        {
            return volume <= 0.0001f ? SilentDecibels : Mathf.Max(SilentDecibels, 20f * Mathf.Log10(volume));
        }

        private static void Load()
        {
            if (mixer != null)
                return;

            mixer = Resources.Load<AudioMixer>(MixerResource);
            musicGroup = mixer.FindMatchingGroups(nameof(AudioChannel.Music))[0];
            effectsGroup = mixer.FindMatchingGroups(nameof(AudioChannel.Effects))[0];
        }
    }
}
