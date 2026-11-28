using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameHub.Common
{
    public static class PlayerScene
    {
        public static string? Pinned { get; set; }

        public static string Name => Pinned ?? SceneManager.GetActiveScene().name;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            Pinned = null;
        }
    }
}
