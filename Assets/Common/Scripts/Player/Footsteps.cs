using System.Collections.Generic;
using UnityEngine;

namespace GameHub.Common
{
    public sealed class Footsteps
    {
        private const string Folder = "Audio/Steps/";
        private const string DefaultSet = "Default";
        private const float Volume = 0.15f;
        private const float StepDistance = 2f;
        private const float PitchRange = 0.1f;
        private const float TeleportDistance = 5f;

        private static readonly Dictionary<string, AudioClip[]> Sets = new();

        private readonly AudioSource source;
        private Vector3 last;
        private float walked;

        public Footsteps(GameObject owner)
        {
            source = owner.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            GameAudio.Route(source, AudioChannel.Effects);
            last = owner.transform.position;
        }

        public void Tick(Vector3 position, bool grounded)
        {
            Vector3 step = position - last;
            step.y = 0f;
            last = position;
            float distance = step.magnitude;
            if (!grounded || distance > TeleportDistance)
                return;

            walked += distance;
            if (walked < StepDistance)
                return;

            walked = 0f;
            AudioClip[] clips = Clips(PlayerScene.Name);
            if (clips.Length == 0)
                return;

            source.pitch = 1f + Random.Range(-PitchRange, PitchRange);
            source.PlayOneShot(clips[Random.Range(0, clips.Length)], Volume);
        }

        private static AudioClip[] Clips(string scene)
        {
            if (!Sets.TryGetValue(scene, out AudioClip[] clips))
            {
                clips = Resources.LoadAll<AudioClip>(Folder + scene);
                if (clips.Length == 0)
                    clips = Resources.LoadAll<AudioClip>(Folder + DefaultSet);
                Sets[scene] = clips;
            }

            return clips;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            Sets.Clear();
        }
    }
}
