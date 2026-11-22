using UnityEngine;

public class VoiceWindowController : MonoBehaviour
{
    public Transform LeftShutter;
    public Transform RightShutter;

    public float OpenLeft = -7.5f;
    public float OpenRight = 7f;

    public float CloseLeft = -2.85f;
    public float CloseRight = 2.15f;

    public float Speed = 2f;
    private bool isOpen = true;

    public void OpenWindow()
    {
        isOpen = true;
    }

    public void CloseWindow()
    {
        isOpen = false;
    }

    public Vector3 ShutterPositions => new(LeftShutter.localPosition.x, RightShutter.localPosition.x, 0);

    private void Update()
    {
        Vector3 leftPos = LeftShutter.localPosition;
        Vector3 rightPos = RightShutter.localPosition;

        leftPos.x = Mathf.Lerp(leftPos.x, isOpen ? OpenLeft : CloseLeft, Time.deltaTime * Speed);

        rightPos.x = Mathf.Lerp(rightPos.x, isOpen ? OpenRight : CloseRight, Time.deltaTime * Speed);

        LeftShutter.localPosition = leftPos;
        RightShutter.localPosition = rightPos;
    }
}
