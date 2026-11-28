using GameHub.Common;
using UnityEngine;

namespace GameHub.Hub
{
    public class RoomExitTrigger : MonoBehaviour
    {
        [SerializeField] private GameObject environmentToDisable;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(Tags.Player))
                return;

            SceneLoader.Instance?.EnterLoadedScene();
            if (environmentToDisable != null)
                environmentToDisable.SetActive(false);
        }
    }
}
