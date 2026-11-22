using UnityEngine;

public class DoorChainController : MonoBehaviour
{
    [Header("Chain Settings")]
    public Transform chain;
    public Transform door;
    public float chainExtensionSpeed = 5f;
    public float maxChainLength = 5.9f;

    private float originalChainLength;
    private bool isExtending;

    private void Start()
    {
        originalChainLength = chain.localScale.y;
    }

    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.E))
        //     isExtending = true;
        // if (Input.GetKeyDown(KeyCode.F))
        //     isExtending = false;

        ExtendChainAndMoveDoor();
    }

    public void ToggleChainExtension() // TODO: toggle the door light when the door is down
    {
        isExtending = !isExtending;
    }

    // ? isExtending is still true even when the door is down so it can be used for extended checks
    private void ExtendChainAndMoveDoor()
    {
        // TODO: check if door is already extended and ignore this logic if it is
        float delta = chainExtensionSpeed * Time.deltaTime;
        float scaleDelta = delta * 0.5f;

        float currentScaleY = chain.localScale.y;
        float targetScaleY = isExtending
            ? Mathf.Min(currentScaleY + scaleDelta, maxChainLength)
            : Mathf.Max(currentScaleY - scaleDelta, originalChainLength);

        float appliedScaleDelta = targetScaleY - currentScaleY;

        chain.localScale = new Vector3(chain.localScale.x, targetScaleY, chain.localScale.z);
        chain.position += Vector3.down * appliedScaleDelta;
        door.position += Vector3.down * (appliedScaleDelta * 2f);
    }

    public bool IsExtending()
    {
        return isExtending;
    }
}
