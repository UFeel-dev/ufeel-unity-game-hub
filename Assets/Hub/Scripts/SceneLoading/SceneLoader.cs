using System;
using System.Collections;
using System.Collections.Generic;
using GameHub.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameHub.Hub
{
    public class SceneLoader : MonoBehaviour
    {
        private const string SceneRootName = "Root";

        private readonly List<GameObject> deferredCameras = new();
        private RenderSettingsSnapshot? loadedSceneSettings;
        private RoomPortal? portal;
        private string? pendingScene;
        private Action? onPendingUnloaded;
        private Scene hubScene;

        public static SceneLoader? Instance { get; private set; }

        public static bool IsHubActive =>
            SceneManager.GetActiveScene().name == SceneNames.Hub || (Instance != null && Instance.HasPendingRoom);

        public bool IsLoading { get; private set; }

        public bool HasPendingRoom => pendingScene != null;

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

        public void LoadAdditiveSceneAtPosition(string sceneName, Vector3 position, Action? onLoaded = null, Action? onUnloaded = null)
        {
            if (!IsLoading && !HasPendingRoom)
                StartCoroutine(LoadAdditiveScene(sceneName, position, onLoaded, onUnloaded));
        }

        public void EnterLoadedScene()
        {
            if (!HasPendingRoom)
                return;

            loadedSceneSettings?.Apply();
            if (portal != null)
                portal.Enter();

            foreach (GameObject deferredCamera in deferredCameras)
            {
                if (deferredCamera != null)
                    deferredCamera.SetActive(true);
            }

            ClearPendingRoom();
        }

        public void UnloadPendingRoom()
        {
            if (IsLoading || HasPendingRoom)
                StartCoroutine(UnloadWhenLoaded());
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

        private IEnumerator LoadAdditiveScene(string sceneName, Vector3 position, Action? onLoaded, Action? onUnloaded)
        {
            IsLoading = true;
            hubScene = SceneManager.GetActiveScene();

            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            GameObject[] rootObjects = loadedScene.GetRootGameObjects();
            GameObject root = Array.Find(rootObjects, rootObject => rootObject.name == SceneRootName);
            if (root == null)
            {
                HideHub();
                SceneManager.SetActiveScene(loadedScene);
            }
            else
            {
                root.transform.position = position;
                pendingScene = sceneName;
                onPendingUnloaded = onUnloaded;
                DeferUntilEntered(rootObjects);

                RenderSettingsSnapshot hubSettings = RenderSettingsSnapshot.Capture();
                SceneManager.SetActiveScene(loadedScene);
                loadedSceneSettings = RenderSettingsSnapshot.Capture();
                hubSettings.Apply();

                portal = FindAnyObjectByType<RoomPortal>();
                if (portal != null)
                    portal.Open(position, rootObjects, hubSettings, loadedSceneSettings);
            }

            onLoaded?.Invoke();

            IsLoading = false;
        }

        private IEnumerator UnloadWhenLoaded()
        {
            yield return new WaitWhile(() => IsLoading);
            if (pendingScene == null)
                yield break;

            string sceneName = pendingScene;
            Action? unloaded = onPendingUnloaded;
            if (portal != null)
                portal.Close();

            ClearPendingRoom();
            SceneManager.SetActiveScene(hubScene);
            unloaded?.Invoke();

            yield return SceneManager.UnloadSceneAsync(sceneName);
        }

        private void ClearPendingRoom()
        {
            deferredCameras.Clear();
            loadedSceneSettings = null;
            portal = null;
            pendingScene = null;
            onPendingUnloaded = null;
        }

        private void DeferUntilEntered(GameObject[] rootObjects)
        {
            List<GameObject> roomPlayers = new();
            foreach (GameObject rootObject in rootObjects)
            {
                foreach (Transform child in rootObject.GetComponentsInChildren<Transform>())
                {
                    if (child.CompareTag(Tags.Player))
                        roomPlayers.Add(child.gameObject);
                    else if (child.TryGetComponent(out Camera _))
                        deferredCameras.Add(child.gameObject);
                }
            }

            foreach (GameObject roomPlayer in roomPlayers)
                roomPlayer.SetActive(false);

            foreach (GameObject deferredCamera in deferredCameras)
                deferredCamera.SetActive(false);
        }
    }
}
