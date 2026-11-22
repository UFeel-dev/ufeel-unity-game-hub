using System;
using System.Collections;
using GameHub.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameHub.Hub
{
    public class SceneLoader : MonoBehaviour
    {
        private const string SceneRootName = "Root";

        public static SceneLoader? Instance { get; private set; }

        public bool IsLoading { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void LoadAdditiveSceneAtPosition(string sceneName, Vector3 position, Action? onLoaded = null)
        {
            onLoaded ??= () => { };

            if (!IsLoading)
                StartCoroutine(LoadAdditiveScene(sceneName, position, onLoaded));
        }

        private static void HideHub()
        {
            GameObject player = GameObject.FindGameObjectWithTag(Tags.Player);
            if (player != null)
                player.SetActive(false);

            GameObject environment = GameObject.FindGameObjectWithTag(Tags.Environment);
            if (environment != null)
                environment.SetActive(false);
        }

        private IEnumerator LoadAdditiveScene(string sceneName, Vector3 position, Action onLoaded)
        {
            IsLoading = true;

            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            GameObject root = Array.Find(loadedScene.GetRootGameObjects(), rootObject => rootObject.name == SceneRootName);
            if (root != null)
                root.transform.position = position;
            else
                HideHub();

            SceneManager.SetActiveScene(loadedScene);
            onLoaded?.Invoke();

            IsLoading = false;
        }
    }
}
