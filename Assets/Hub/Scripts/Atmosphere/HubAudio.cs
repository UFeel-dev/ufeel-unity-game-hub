using GameHub.Common;
using UnityEngine;

namespace GameHub.Hub
{
    public sealed class HubAudio
    {
        private const float AmbientVolume = 0.4f;
        private const float AmbientFade = 0.8f;
        private const float HumVolume = 0.28f;
        private const float HumFade = 2f;
        private const float MenuVolume = 0.28f;
        private const float StepVolume = 0.24f;
        private const float StepDistance = 2f;
        private const float StepPitchRange = 0.1f;
        private const float TeleportDistance = 5f;

        private readonly HubSoundSet sounds;
        private readonly AudioSource ambient;
        private readonly AudioSource menu;
        private readonly AudioSource hum;
        private readonly AudioSource steps;
        private Vector3 lastStep;
        private float walked;

        public HubAudio(Transform owner, HubSoundSet sounds)
        {
            this.sounds = sounds;
            GameObject root = new("HubAudio");
            root.transform.SetParent(owner, false);

            ambient = CreateSource(root.transform, "Ambient", AudioChannel.Music);
            ambient.clip = sounds.Ambient;
            ambient.loop = true;
            ambient.volume = 0f;
            ambient.Play();

            hum = CreateSource(root.transform, "CarouselHum", AudioChannel.Effects);
            hum.clip = sounds.CarouselHum;
            hum.loop = true;
            hum.volume = 0f;
            hum.Play();

            menu = CreateSource(root.transform, "Menu", AudioChannel.Effects);
            steps = CreateSource(root.transform, "Steps", AudioChannel.Effects);
        }

        public void Tick(float deltaTime, bool hubActive, bool carouselTurning, Vector3 playerPosition)
        {
            ambient.volume = Mathf.MoveTowards(ambient.volume, hubActive ? AmbientVolume : 0f, AmbientFade * deltaTime);
            hum.volume = Mathf.MoveTowards(hum.volume, hubActive && carouselTurning ? HumVolume : 0f, HumFade * deltaTime);
            if (hubActive)
                Walk(playerPosition);
            else
                lastStep = playerPosition;
        }

        public void MenuMove() => menu.PlayOneShot(sounds.MenuMove, MenuVolume);

        public void MenuConfirm() => menu.PlayOneShot(sounds.MenuConfirm, MenuVolume);

        public void MenuBack() => menu.PlayOneShot(sounds.MenuBack, MenuVolume);

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

        private void Walk(Vector3 position)
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
    }
}
