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
            if (SurvivorGameManager.Instance == null || !SurvivorGameManager.Instance.IsPlaying)
                return;

            if (EyeInput.Instance != null && EyeInput.Instance.RightPressed)
            {
                focusedIndex = (focusedIndex + 1) % buttons.Length;
                GameSounds.Play(SoundCue.MenuMove);
                UpdateFocus();
            }

            if (EyeInput.Instance != null && EyeInput.Instance.LeftPressed)
            {
                focusedIndex = (focusedIndex - 1 + buttons.Length) % buttons.Length;
                GameSounds.Play(SoundCue.MenuMove);
                UpdateFocus();
            }

            if (BlinkDetector.Instance != null && BlinkDetector.Instance.DoubleBlinkPressed)
            {
                GameSounds.Play(SoundCue.MenuConfirm);
                buttons[focusedIndex].Select();
            }
        }

        private static void OnButtonSelected(string buttonName)
        {
            if (buttonName.Contains("Start") || buttonName.Contains("Restart"))
            {
                if (SurvivorGameManager.Instance != null)
                    SurvivorGameManager.Instance.StartGame();
            }
            else if (buttonName.Contains("Quit"))
            {
                GameFlow.ReturnToHub();
            }
        }

        private void UpdateFocus()
        {
            for (int i = 0; i < buttons.Length; i++)
                buttons[i].SetFocused(i == focusedIndex);
        }
    }
}
