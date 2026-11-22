using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DoorSelectionManager : MonoBehaviour
{
    public CarouselRotator carouselRotator;
    public Transform doorHoldersParent;
    public TMP_InputField doorIDInput;
    public TMP_Dropdown doorNameDropdown;

    private string currentDropdownSelection = string.Empty;
    private DoorChainController savedChainControllers = null;

    private void Awake()
    {
        SetupDoorDropdownOptions();
    }

    public void SetupDoorDropdownOptions()
    {
        List<string> options = new() { string.Empty };

        foreach (Transform child in doorHoldersParent)
        {
            if (child.TryGetComponent(out DoorIdentifier doorIDComponent) && doorIDComponent.doorName != string.Empty)
            {
                string doorName = doorIDComponent.doorName;
                options.Add(doorName);
            }
        }

        doorNameDropdown.ClearOptions();
        doorNameDropdown.AddOptions(options);

        doorNameDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    private void OnDropdownValueChanged(int index)
    {
        currentDropdownSelection = doorNameDropdown.options[index].text;
    }

    public void SelectDoorFromInput()
    {
        if (int.TryParse(doorIDInput.text, out int id))
        {
            SelectDoorByID(id);
        }
        else
        {
            Debug.LogWarning("Invalid Door ID input.");
        }
    }

    public void SelectDoor()
    {
        if (currentDropdownSelection != string.Empty)
        {
            SelectDoorFromDropdown();
        }
        else
        {
            SelectDoorFromInput();
        }
    }

    public void SelectDoorFromDropdown()
    {
        // TODO: check if we can setup enum instead of strings, or anything cleaner and more specific at least
        SelectDoorByName(currentDropdownSelection);
    }

    public void SelectDoorByID(int id)
    {
        foreach (Transform child in doorHoldersParent)
        {
            if (child.TryGetComponent(out DoorIdentifier doorIDComponent) && doorIDComponent.doorID == id)
            {
                carouselRotator.RotateToDoor(id);
                StartCoroutine(WaitAndDropDoor(child));
                return;
            }
        }

        Debug.LogWarning("Door ID not found: " + id);
    }

    public void SelectDoorByName(string name)
    {
        foreach (Transform child in doorHoldersParent)
        {
            if (child.TryGetComponent(out DoorIdentifier doorIDComponent) && doorIDComponent.doorName == name)
            {
                carouselRotator.RotateToDoor(doorIDComponent.doorID);
                StartCoroutine(WaitAndDropDoor(child));
                return;
            }
        }
    }

    public string SelectedDoorName() => currentDropdownSelection;

    public void SetSelectedDoor(int dropdownIndex)
    {
        if (dropdownIndex < 0 || dropdownIndex >= doorNameDropdown.options.Count)
            return;

        doorNameDropdown.value = dropdownIndex;
        doorNameDropdown.RefreshShownValue();
    }

    public void SelectNextDoor()
    {
        if (doorNameDropdown.options.Count <= 1)
            return;

        int next = doorNameDropdown.value + 1;

        if (next >= doorNameDropdown.options.Count)
            next = 1;

        SetSelectedDoor(next);
    }

    public void SelectPreviousDoor()
    {
        if (doorNameDropdown.options.Count <= 1)
            return;

        int previous = doorNameDropdown.value - 1;

        if (previous < 1)
            previous = doorNameDropdown.options.Count - 1;

        SetSelectedDoor(previous);
    }

    public bool HasSelection()
    {
        return doorNameDropdown.value != 0;
    }

    public void ConfirmCurrentSelection()
    {
        if (!HasSelection())
            return;

        SelectDoorFromDropdown();
    }

    private System.Collections.IEnumerator WaitAndDropDoor(Transform doorHolder)
    {
        if (savedChainControllers?.IsExtending() ?? false)
        {
            savedChainControllers.ToggleChainExtension();
        }
        yield return new WaitUntil(() => !carouselRotator.rotateToTarget);
        DoorChainController chainController = doorHolder.GetComponentInChildren<DoorChainController>();
        if (chainController != null)
        {
            chainController.ToggleChainExtension();
            savedChainControllers = chainController;
        }
    }
}
