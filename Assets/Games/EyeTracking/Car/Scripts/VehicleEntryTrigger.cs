using GameHub.Common;
using UnityEngine;

namespace GameHub.Car
{
    public class VehicleEntryTrigger : MonoBehaviour
    {
        [SerializeField] private VehicleController vehicleController;
        [SerializeField] private Camera vehicleCamera;

        private GameObject player;
        private bool isInVehicle;

        private void Start()
        {
            player = GameObject.FindWithTag(Tags.Player);
            SetVehicleActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isInVehicle && other.CompareTag(Tags.Player))
                EnterVehicle();
        }

        private void EnterVehicle()
        {
            isInVehicle = true;
            player.SetActive(false);
            SetVehicleActive(true);

            Camera mainCamera = Camera.main;
            if (vehicleCamera != null && mainCamera != null && mainCamera != vehicleCamera)
                mainCamera.enabled = false;
        }

        private void SetVehicleActive(bool active)
        {
            if (vehicleController != null)
                vehicleController.enabled = active;

            if (vehicleCamera != null)
                vehicleCamera.enabled = active;
        }
    }
}
