using GameHub.Common;
using UnityEngine;

namespace GameHub.Hub
{
    public sealed class HubAudio
    {
        private const float HumVolume = 0.28f;
        private const float HumFade = 2f;

        private readonly AudioSource hum;

        public HubAudio(Transform owner, HubSoundSet sounds)
        {
            GameObject root = new("HubAudio");
            root.transform.SetParent(owner, false);

            hum = CreateSource(root.transform, "CarouselHum", AudioChannel.Effects);
            hum.clip = sounds.CarouselHum;
            hum.loop = true;
            hum.volume = 0f;
            hum.Play();
        }

        public void Tick(float deltaTime, bool hubActive, bool carouselTurning)
        {
            hum.volume = Mathf.MoveTowards(hum.volume, hubActive && carouselTurning ? HumVolume : 0f, HumFade * deltaTime);
        }

        private static AudioSource CreateSource(Transform parent, string name, AudioChannel channel)
        {
            GameObject holder = new(name);
            holder.transform.SetParent(parent, false);
            AudioSource source = holder.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            GameAudio.Route(source, channel);
            return source;
        }
    }
}
