using UnityEngine;

public class RoomExitTrigger : MonoBehaviour
{
    [SerializeField] private GameObject environmentToDisable;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (environmentToDisable != null)
        {
            environmentToDisable.SetActive(false);
            // Debug.Log("Environment disabled after player passed through the door.");
        }
    }
}
