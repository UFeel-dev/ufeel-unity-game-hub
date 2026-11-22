using UFeel;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameHub.Common
{
    public static class GameFlow
    {
        public static void ReturnToHub()
        {
            UFeelAPI.SpeechToText.StopDetection();

            if (SceneManager.GetActiveScene().name != SceneNames.Hub)
                SceneManager.LoadScene(SceneNames.Hub);
        }

        public static void Quit()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit();
#endif
        }
    }
}
