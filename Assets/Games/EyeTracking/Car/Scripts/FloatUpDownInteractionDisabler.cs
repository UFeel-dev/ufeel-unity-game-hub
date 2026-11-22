using UnityEngine;

public class FloatUpDownInteractionDisabler : MonoBehaviour
{
    [SerializeField] private FloatUpDown scriptToDisable;

    private bool hasInteracted = false;

    public void OnInteract()
    {
        if (hasInteracted)
            return;

        hasInteracted = true;

        if (scriptToDisable != null)
            scriptToDisable.enabled = false;
        else
            Debug.LogWarning("No script assigned to disable.");
    }
}
