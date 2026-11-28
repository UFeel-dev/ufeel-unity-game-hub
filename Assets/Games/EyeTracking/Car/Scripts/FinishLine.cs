using GameHub.Common;
using UnityEngine;

namespace GameHub.Car
{
    public class FinishLine : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(Tags.Vehicle))
                return;

            GameSounds.Play(SoundCue.Win);
            GameFlow.ReturnToHub();
        }
    }
}
