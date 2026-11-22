using UnityEngine;

public class VoiceDoorController : MonoBehaviour
{
    public Transform Pivot;
    public float OpenAngle = 90f;
    public float Speed = 2f;

    private bool isOpen = false;

    public void OpenDoor()
    {
        isOpen = true;
    }

    public void CloseDoor()
    {
        isOpen = false;
    }

    private void Update()
    {
        float targetY = isOpen ? OpenAngle : 0f;
        Vector3 targetRotation = new(0, targetY, 0);

        Pivot.localRotation = Quaternion.Lerp(
            Pivot.localRotation,
            Quaternion.Euler(targetRotation),
            Time.deltaTime * Speed
        );
    }
}
