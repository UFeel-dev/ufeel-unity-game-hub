using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GameHub.Hub
{
    public class DoorSelectionManager : MonoBehaviour
    {
        [SerializeField] private CarouselRotator carouselRotator;
        [SerializeField] private Transform doorHoldersParent;
        [SerializeField] private TMP_InputField doorIdInput;
        [SerializeField] private TMP_Dropdown doorNameDropdown;

        private DoorChainController loweredChain;

        private bool HasDropdownSelection => doorNameDropdown.value != 0;

        private void Awake()
        {
            List<string> options = new() { string.Empty };
            foreach (Transform child in doorHoldersParent)
            {
                if (child.TryGetComponent(out DoorIdentifier identifier) && !string.IsNullOrEmpty(identifier.DoorName))
                    options.Add(identifier.DoorName);
            }

            doorNameDropdown.ClearOptions();
            doorNameDropdown.AddOptions(options);
        }

        public void SelectDoor()
        {
            if (HasDropdownSelection)
            {
                ConfirmCurrentSelection();
                return;
            }

            if (int.TryParse(doorIdInput.text, out int id))
                SelectDoorWhere(identifier => identifier.DoorId == id);
            else
                Debug.LogWarning($"Invalid door ID: '{doorIdInput.text}'.", this);
        }

        public void ConfirmCurrentSelection()
        {
            if (!HasDropdownSelection)
                return;

            string selectedName = doorNameDropdown.options[doorNameDropdown.value].text;
            SelectDoorWhere(identifier => identifier.DoorName == selectedName);
        }

        public void SelectNextDoor()
        {
            int count = doorNameDropdown.options.Count;
            if (count <= 1)
                return;

            int next = doorNameDropdown.value + 1;
            SetDropdownValue(next >= count ? 1 : next);
        }

        public void SelectPreviousDoor()
        {
            int count = doorNameDropdown.options.Count;
            if (count <= 1)
                return;

            int previous = doorNameDropdown.value - 1;
            SetDropdownValue(previous < 1 ? count - 1 : previous);
        }

        private void SetDropdownValue(int value)
        {
            doorNameDropdown.value = value;
            doorNameDropdown.RefreshShownValue();
        }

        private void SelectDoorWhere(System.Predicate<DoorIdentifier> match)
        {
            foreach (Transform child in doorHoldersParent)
            {
                if (child.TryGetComponent(out DoorIdentifier identifier) && match(identifier))
                {
                    carouselRotator.RotateToDoor(identifier.DoorId);
                    StartCoroutine(LowerDoorWhenAligned(child));
                    return;
                }
            }

            Debug.LogWarning("No door matches the selection.", this);
        }

        private IEnumerator LowerDoorWhenAligned(Transform doorHolder)
        {
            if (loweredChain != null && loweredChain.IsExtending)
                loweredChain.ToggleExtension();

            yield return new WaitUntil(() => !carouselRotator.IsRotatingToTarget);

            DoorChainController chain = doorHolder.GetComponentInChildren<DoorChainController>();
            if (chain == null)
                yield break;

            chain.ToggleExtension();
            loweredChain = chain;
        }
    }
}
