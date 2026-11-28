using System.Collections.Generic;
using UnityEngine;

namespace GameHub.Common
{
    public sealed class SceneMusic : MonoBehaviour
    {
        private const string Folder = "Music/";
        private const float Volume = 0.45f;
        private const float FadeSeconds = 2f;

        private static readonly Dictionary<string, AudioClip?> Tracks = new();

        private static SceneMusic? instance;

        private readonly AudioSource[] sources = new AudioSource[2];
        private int current;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            Tracks.Clear();
            if (instance == null)
                new GameObject(nameof(SceneMusic)).AddComponent<SceneMusic>();
        }

        private static AudioClip? Track(string scene)
        {
            if (!Tracks.TryGetValue(scene, out AudioClip? clip))
            {
                clip = Resources.Load<AudioClip>(Folder + scene);
                Tracks[scene] = clip;
            }

            return clip;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            for (int i = 0; i < sources.Length; i++)
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = true;
                source.spatialBlend = 0f;
                source.volume = 0f;
                GameAudio.Route(source, AudioChannel.Music);
                sources[i] = source;
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        private void Update()
        {
            AudioClip? wanted = Track(PlayerScene.Name);
            if (sources[current].clip != wanted)
                CrossTo(wanted);

            float step = Volume / FadeSeconds * Time.unscaledDeltaTime;
            for (int i = 0; i < sources.Length; i++)
            {
                AudioSource source = sources[i];
                float target = i == current && source.clip != null ? Volume : 0f;
                source.volume = Mathf.MoveTowards(source.volume, target, step);
                if (i != current && source.volume <= 0f && source.isPlaying)
                    source.Stop();
            }
        }

        private void CrossTo(AudioClip? wanted)
        {
            current = 1 - current;
            AudioSource next = sources[current];
            if (next.clip != wanted)
            {
                next.Stop();
                next.clip = wanted;
                next.volume = 0f;
            }

            if (wanted != null && !next.isPlaying)
                next.Play();
        }
    }
}
