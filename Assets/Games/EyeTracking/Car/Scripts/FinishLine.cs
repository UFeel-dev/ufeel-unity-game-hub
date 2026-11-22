using GameHub.Common;
using UnityEngine;

namespace GameHub.Car
{
    public class FinishLine : MonoBehaviour
    {
        [SerializeField] private VehicleEntryTrigger vehicleEntryTrigger;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(Tags.Vehicle))
                return;

            vehicleEntryTrigger.ExitVehicle();
            GameFlow.ReturnToHub();
        }
    }
}
