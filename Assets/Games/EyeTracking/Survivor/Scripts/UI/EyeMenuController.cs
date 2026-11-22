using GameHub.Common;
using UnityEngine;

namespace GameHub.Survivor
{
    public class EyeMenuController : MonoBehaviour
    {
        [SerializeField] private EyeMenuButton[] buttons;

        private int focusedIndex;

        private void Start()
        {
            foreach (EyeMenuButton button in buttons)
            {
                string buttonName = button.gameObject.name;
                button.Selected += () => OnButtonSelected(buttonName);
            }

            UpdateFocus();
        }

        private void Update()
        {
            if (SurvivorGameManager.Instance.IsPlaying)
                return;

            if (EyeInput.Instance.RightPressed)
            {
                focusedIndex = (focusedIndex + 1) % buttons.Length;
                UpdateFocus();
            }

            if (EyeInput.Instance.LeftPressed)
            {
                focusedIndex = (focusedIndex - 1 + buttons.Length) % buttons.Length;
                UpdateFocus();
            }

            if (BlinkDetector.Instance.DoubleBlinkPressed)
                buttons[focusedIndex].Select();
        }

        private static void OnButtonSelected(string buttonName)
        {
            if (buttonName.Contains("Start") || buttonName.Contains("Restart"))
                SurvivorGameManager.Instance.StartGame();
            else if (buttonName.Contains("Quit"))
                GameFlow.ReturnToHub();
        }

        private void UpdateFocus()
        {
            for (int i = 0; i < buttons.Length; i++)
                buttons[i].SetFocused(i == focusedIndex);
        }
    }
}
