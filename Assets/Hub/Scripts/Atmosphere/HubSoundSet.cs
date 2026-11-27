using System.Collections.Generic;
using UnityEngine;

namespace GameHub.Hub
{
    [CreateAssetMenu(fileName = "HubSounds", menuName = "GameHub/Hub/Sound Set")]
    public sealed class HubSoundSet : ScriptableObject
    {
        [SerializeField] private AudioClip ambient;
        [SerializeField] private AudioClip menuMove;
        [SerializeField] private AudioClip menuConfirm;
        [SerializeField] private AudioClip menuBack;
        [SerializeField] private AudioClip carouselHum;
        [SerializeField] private List<AudioClip> steps = new();

        public AudioClip Ambient => ambient;

        public AudioClip MenuMove => menuMove;

        public AudioClip MenuConfirm => menuConfirm;

        public AudioClip MenuBack => menuBack;

        public AudioClip CarouselHum => carouselHum;

        public IReadOnlyList<AudioClip> Steps => steps;
    }
}
