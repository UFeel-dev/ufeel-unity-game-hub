using UFeel;
using UnityEngine;

namespace GameHub.Common
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject pauseMenuUI;
        [SerializeField] private FirstPersonController playerController;

        private bool disabledPlayer;

        public static bool IsPaused { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticsWithoutDomainReload()
        {
            IsPaused = false;
        }

        private void Update()
        {
            if (!LayoutInput.GetKeyDown(KeyCode.M))
                return;

            if (IsPaused)
                Resume();
            else
                Pause();
        }

        public void Resume()
        {
            UFeelAPI.SpeechToText.HideMicrophoneSelector();
            pauseMenuUI.SetActive(false);
            Time.timeScale = 1f;

            if (disabledPlayer)
            {
                playerController.enabled = true;
                disabledPlayer = false;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            IsPaused = false;
        }

        public void Pause()
        {
            pauseMenuUI.SetActive(true);
            Time.timeScale = 0f;

            if (playerController.enabled)
            {
                playerController.enabled = false;
                disabledPlayer = true;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            UFeelAPI.SpeechToText.ShowMicrophoneSelector();
            IsPaused = true;
        }

        public void LoadLobby()
        {
            Resume();
            GameFlow.ReturnToHub();
        }

        public static void QuitGame()
        {
            GameFlow.Quit();
        }
    }
}
