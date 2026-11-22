using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    private bool isLoading;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadSceneAsync(string sceneName, float delay = 0.0f)
    {
        if (!isLoading)
        {
            StartCoroutine(LoadSceneCoroutine(sceneName, delay));
        }
    }

    private IEnumerator LoadSceneCoroutine(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);

        isLoading = true;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        isLoading = false;
    }

    public void LoadAdditiveSceneAtPosition(string sceneName, Vector3 targetPosition, System.Action onLoaded = null)
    {
        if (!isLoading)
        {
            StartCoroutine(LoadAdditiveSceneCoroutine(sceneName, targetPosition, onLoaded));
        }
    }

    private IEnumerator LoadAdditiveSceneCoroutine(string sceneName, Vector3 targetPosition, System.Action onLoaded)
    {
        isLoading = true;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        while (!asyncLoad.isDone)
            yield return null;

        Scene loadedScene = SceneManager.GetSceneByName(sceneName);
        while (!loadedScene.isLoaded)
            yield return null;

        bool foundRoot = false;

        foreach (GameObject rootObj in loadedScene.GetRootGameObjects())
        {
            if (rootObj.name == "Root") // TODO: check if this main object detection logic is appropriate
            {
                rootObj.transform.position = targetPosition;
                foundRoot = true;
                break;
            }
        }

        if (!foundRoot)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerObject.SetActive(false);
            }

            GameObject environmentObject = GameObject.FindGameObjectWithTag("Environment");
            if (environmentObject != null)
            {
                environmentObject.SetActive(false);
            }
        }

        ApplySceneLighting(loadedScene);
        onLoaded?.Invoke();

        isLoading = false;
    }

    private static void ApplySceneLighting(Scene scene)
    {
        SceneManager.SetActiveScene(scene);
    }

    public void UnloadAdditiveScene(string sceneName, System.Action complete = null)
    {
        if (!isLoading)
        {
            StartCoroutine(UnloadAdditiveSceneCoroutine(sceneName, complete));
        }
    }

    private IEnumerator UnloadAdditiveSceneCoroutine(string sceneName, System.Action complete)
    {
        isLoading = true; // TODO: check if we should set loading to true while unloading.
                          // -> probably important to avoid spamming unload
                          // -> but the "isLoading" name is not appropriate in this case as it handles both loading and unloading

        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneName);

        while (!asyncUnload.isDone)
            yield return null;

        isLoading = false;

        Scene testingRoomScene = SceneManager.GetSceneByName("GameHub");
        ApplySceneLighting(testingRoomScene);
        complete?.Invoke();
    }

    public bool IsLoading()
    {
        return isLoading;
    }
}

// TODO: perform fade-out transition
