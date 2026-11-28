using GameHub.Common;
using UnityEngine;

namespace GameHub.Hub
{
    public class RoomExitTrigger : MonoBehaviour
    {
        [SerializeField] private GameObject environmentToDisable;

        private void OnTriggerEnter(Collider other)
        {
            SceneLoader? loader = SceneLoader.Instance;
            if (!other.CompareTag(Tags.Player) || loader == null || !loader.HasPendingRoom)
                return;

            loader.EnterLoadedScene();
            if (environmentToDisable != null)
                environmentToDisable.SetActive(false);
        }
    }
}
