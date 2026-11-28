using UnityEngine;

namespace GameHub.Hub
{
    [CreateAssetMenu(fileName = "HubSounds", menuName = "GameHub/Hub/Sound Set")]
    public sealed class HubSoundSet : ScriptableObject
    {
        [SerializeField] private AudioClip carouselHum;

        public AudioClip CarouselHum => carouselHum;
    }
}
