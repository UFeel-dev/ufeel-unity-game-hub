using GameHub.Common;
using UnityEngine;

namespace GameHub.Hub
{
    public class HubAtmosphere : MonoBehaviour
    {
        [SerializeField] private HubSoundSet sounds;
        [SerializeField] private CarouselRotator carousel;
        [SerializeField] private Transform player;

        private HubWelcome? welcome;

        public HubAudio Audio { get; private set; }

        private void Awake()
        {
            Audio = new HubAudio(transform, sounds);
        }

        private void Start()
        {
            welcome = HubWelcome.ShowOnFirstVisit(UiKit.Shared, transform);
        }

        private void OnDestroy()
        {
            welcome?.Close();
        }

        private void Update()
        {
            bool hubActive = SceneLoader.IsHubActive;
            Audio.Tick(Time.deltaTime, hubActive, carousel.IsRotatingToTarget, player.position);

            if (welcome != null && welcome.Tick(Time.unscaledDeltaTime))
                welcome = null;
        }
    }
}
