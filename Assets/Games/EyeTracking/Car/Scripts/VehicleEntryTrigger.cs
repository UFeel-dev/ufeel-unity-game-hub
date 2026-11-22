using UnityEngine;

public class VehicleEntryTrigger : MonoBehaviour
{
    [SerializeField] private MonoBehaviour vehicleController;
    [SerializeField] private Camera vehicleCamera;

    private GameObject playerObject;
    private bool hasEntered = false;

    [SerializeField] private Vector3 exitOffset = new(2f, 0f, 0f);

    private void Start()
    {
        playerObject = GameObject.FindWithTag("Player");

        if (vehicleController != null)
            vehicleController.enabled = false;

        if (vehicleCamera != null)
            vehicleCamera.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasEntered) return;

        if (other.CompareTag("Player"))
        {
            hasEntered = true;
            EnterVehicle();
        }
    }

    private void Update()
    {
        if (hasEntered && LayoutInput.GetKeyDown(KeyCode.E))
        {
            ExitVehicle();
        }
    }

    private void EnterVehicle()
    {
        playerObject.SetActive(false);

        if (vehicleController != null)
            vehicleController.enabled = true;

        if (vehicleCamera != null)
        {
            vehicleCamera.enabled = true;

            Camera mainCam = Camera.main;
            if (mainCam != null && mainCam != vehicleCamera)
                mainCam.enabled = false;
        }

        Debug.Log("Entered vehicle.");
    }

    public GameObject ExitVehicle()
    {
        hasEntered = false;

        if (playerObject != null)
        {
            playerObject.SetActive(true);
            playerObject.transform.position = transform.position + exitOffset;
        }

        if (vehicleController != null)
            vehicleController.enabled = false;

        if (vehicleCamera != null)
            vehicleCamera.enabled = false;

        Debug.Log("Exited vehicle.");

        return playerObject;
    }
}
