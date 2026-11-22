using UnityEngine;

public class FinishLine : MonoBehaviour
{
    [SerializeField] private VehicleEntryTrigger vehicleEntryTrigger;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Vehicle")) return;

        GameObject playerObject = vehicleEntryTrigger.ExitVehicle();
        if (!playerObject) return;

        PauseMenu.GoToLobby();
    }
}
