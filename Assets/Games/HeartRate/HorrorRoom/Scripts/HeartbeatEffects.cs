using System.Collections;
using GameHub.Common;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.HorrorRoom
{
    public class HeartbeatEffects : MonoBehaviour
    {
        private const float FilterAlpha = 0.25f;

        [SerializeField] private AudioSource heartbeatAudio;
        [SerializeField] private Image redFilter;

        private bool isFilterVisible;

        private void Start()
        {
            SetFilterVisible(false);
            StartCoroutine(PlayHeartbeat());
        }

        private IEnumerator PlayHeartbeat()
        {
            while (true)
            {
                if (!HeartRateTempo.TryGetBpm(out uint bpm))
                {
                    yield return null;
                    continue;
                }

                heartbeatAudio.Play();
                SetFilterVisible(!isFilterVisible);
                yield return new WaitForSeconds(60f / bpm);
            }
        }

        private void SetFilterVisible(bool visible)
        {
            isFilterVisible = visible;
            redFilter.color = new Color(1f, 0f, 0f, visible ? FilterAlpha : 0f);
        }
    }
}
