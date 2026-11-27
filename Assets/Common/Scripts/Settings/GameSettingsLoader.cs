using UnityEngine;

namespace GameHub.Common
{
    public sealed class GameSettingsLoader : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            GameObject holder = new(nameof(GameSettingsLoader));
            DontDestroyOnLoad(holder);
            holder.AddComponent<GameSettingsLoader>();
        }

        private void Start()
        {
            GameSettings.Apply();
            Destroy(gameObject);
        }
    }
}
