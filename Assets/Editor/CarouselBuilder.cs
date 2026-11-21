using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarouselBuilder : EditorWindow
{
    private const string LayoutPath = "Assets/Editor/CarouselLayout.json";

    [System.Serializable]
    public class SpecialDoorConfig
    {
        public int doorIndex;
        public string doorName;
        public string targetSceneName;
        public Color doorColor = Color.red;
        public bool inProgress;
    }

    [System.Serializable]
    private class CarouselLayout
    {
        public string doorHolderPrefab;
        public string doorHoldersPath;
        public string wallToDisablePath;
        public int doorCount = 44;
        public float radius = 19.7f;
        public List<SpecialDoorConfig> specialDoors = new();
    }

    private GameObject doorHolderPrefab;
    private Transform carouselTop;
    private GameObject wallToDisable;
    private int doorCount = 44;
    private float radius = 19.7f;
    private List<SpecialDoorConfig> specialDoors = new();
    private Vector2 scrollPos;

    [MenuItem("Tools/Build Carousel")]
    private static void Init()
    {
        CarouselBuilder window = GetWindow<CarouselBuilder>();
        window.titleContent = new GUIContent("Carousel Builder");
        window.minSize = new Vector2(420f, 600f);
        window.Show();
    }

    private void OnEnable()
    {
        if (File.Exists(LayoutPath))
            LoadLayout();
    }

    private void OnGUI()
    {
        GUILayout.Label("Carousel Door Placer", EditorStyles.boldLabel);

        doorHolderPrefab = (GameObject)EditorGUILayout.ObjectField("Door Holder Prefab", doorHolderPrefab, typeof(GameObject), false);
        carouselTop = (Transform)EditorGUILayout.ObjectField("Door Holder Container (Parent)", carouselTop, typeof(Transform), true);
        wallToDisable = (GameObject)EditorGUILayout.ObjectField("Wall To Disable", wallToDisable, typeof(GameObject), true);
        doorCount = EditorGUILayout.IntField("Number of Doors", doorCount);
        radius = EditorGUILayout.FloatField("Radius", radius);

        EditorGUILayout.Space();
        GUILayout.Label("Special Doors", EditorStyles.boldLabel);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));
        for (int i = 0; i < specialDoors.Count; i++)
        {
            SpecialDoorConfig config = specialDoors[i];
            EditorGUILayout.BeginVertical("box");
            config.doorIndex = EditorGUILayout.IntField("Door Index", config.doorIndex);
            config.doorName = EditorGUILayout.TextField("Door Name", config.doorName);
            config.targetSceneName = EditorGUILayout.TextField("Target Scene", config.targetSceneName);
            config.doorColor = EditorGUILayout.ColorField("Door Color", config.doorColor);
            config.inProgress = EditorGUILayout.Toggle(DoorTrigger.InProgressLabel, config.inProgress);

            if (!config.inProgress && !IsSceneInBuild(config.targetSceneName))
                EditorGUILayout.HelpBox($"Scene '{config.targetSceneName}' is not in the Build Settings: the door will show '{DoorTrigger.InProgressLabel}' and stay closed.", MessageType.Warning);

            if (GUILayout.Button("Remove"))
                specialDoors.RemoveAt(i);
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Add Special Door"))
            specialDoors.Add(new SpecialDoorConfig());

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Load Layout"))
            LoadLayout();
        if (GUILayout.Button("Save Layout"))
            SaveLayout();
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Load Special Doors From Scene"))
        {
            if (carouselTop == null)
            {
                Debug.LogError("Carousel parent is not assigned.");
                return;
            }

            LoadSpecialDoorsFromScene();
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Build Carousel"))
        {
            if (doorHolderPrefab == null || carouselTop == null)
            {
                Debug.LogError("Missing prefab or parent.");
                return;
            }

            BuildCarousel();
        }
    }

    private static bool IsSceneInBuild(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return false;

        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled && Path.GetFileNameWithoutExtension(scene.path) == sceneName)
                return true;
        }
        return false;
    }

    private static string GetHierarchyPath(Transform target)
    {
        if (target == null)
            return string.Empty;

        string path = target.name;
        for (Transform parent = target.parent; parent != null; parent = parent.parent)
            path = $"{parent.name}/{path}";
        return path;
    }

    private static Transform FindByHierarchyPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        int separator = path.IndexOf('/');
        string rootName = separator < 0 ? path : path[..separator];
        string rest = separator < 0 ? string.Empty : path[(separator + 1)..];

        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (root.name != rootName)
                continue;

            Transform found = rest.Length == 0 ? root.transform : root.transform.Find(rest);
            if (found != null)
                return found;
        }
        return null;
    }

    private void LoadLayout()
    {
        if (!File.Exists(LayoutPath))
        {
            Debug.LogError($"Carousel layout not found: {LayoutPath}");
            return;
        }

        CarouselLayout layout = JsonUtility.FromJson<CarouselLayout>(File.ReadAllText(LayoutPath));
        doorHolderPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(layout.doorHolderPrefab);
        carouselTop = FindByHierarchyPath(layout.doorHoldersPath);
        Transform wall = FindByHierarchyPath(layout.wallToDisablePath);
        wallToDisable = wall != null ? wall.gameObject : null;
        doorCount = layout.doorCount;
        radius = layout.radius;
        specialDoors = layout.specialDoors ?? new List<SpecialDoorConfig>();
        specialDoors.Sort((a, b) => a.doorIndex.CompareTo(b.doorIndex));

        if (carouselTop == null)
            Debug.LogWarning($"Door holder container '{layout.doorHoldersPath}' not found in the open scene.");

        Repaint();
        Debug.Log($"Loaded carousel layout with {specialDoors.Count} special doors from {LayoutPath}.");
    }

    private void SaveLayout()
    {
        specialDoors.Sort((a, b) => a.doorIndex.CompareTo(b.doorIndex));
        CarouselLayout layout = new()
        {
            doorHolderPrefab = doorHolderPrefab != null ? AssetDatabase.GetAssetPath(doorHolderPrefab) : string.Empty,
            doorHoldersPath = GetHierarchyPath(carouselTop),
            wallToDisablePath = wallToDisable != null ? GetHierarchyPath(wallToDisable.transform) : string.Empty,
            doorCount = doorCount,
            radius = radius,
            specialDoors = specialDoors
        };

        File.WriteAllText(LayoutPath, JsonUtility.ToJson(layout, true) + "\n");
        AssetDatabase.ImportAsset(LayoutPath);
        Debug.Log($"Saved carousel layout with {specialDoors.Count} special doors to {LayoutPath}.");
    }

    private void LoadSpecialDoorsFromScene()
    {
        specialDoors.Clear();

        foreach (Transform child in carouselTop)
        {
            if (!child.TryGetComponent(out DoorIdentifier identifier))
                continue;

            if (!child.TryGetComponent(out DoorReferences references))
                continue;

            bool isSpecial =
                !string.IsNullOrWhiteSpace(identifier.doorName) ||
                !string.IsNullOrWhiteSpace(identifier.targetSceneName);

            if (!isSpecial)
                continue;

            specialDoors.Add(new SpecialDoorConfig
            {
                doorIndex = identifier.doorID,
                doorName = identifier.doorName,
                targetSceneName = identifier.targetSceneName,
                doorColor = references.controller.DoorColor,
                inProgress = references.trigger.inProgress
            });
        }

        specialDoors.Sort((a, b) => a.doorIndex.CompareTo(b.doorIndex));

        Repaint();

        Debug.Log($"Loaded {specialDoors.Count} special doors.");
    }

    private void BuildCarousel()
    {
        for (int i = carouselTop.childCount - 1; i >= 0; i--)
        {
            if (Application.isEditor)
                DestroyImmediate(carouselTop.GetChild(i).gameObject);
        }

        float angleStep = 360f / doorCount;

        for (int i = 0; i < doorCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = Mathf.Sin(angle) * radius;
            float z = Mathf.Cos(angle) * radius;

            Vector3 pos = new(x, 0f, z);

            GameObject holder = (GameObject)PrefabUtility.InstantiatePrefab(doorHolderPrefab);
            holder.name = $"DoorHolder_{i}";
            holder.transform.SetParent(carouselTop, false);
            holder.transform.localPosition = pos;

            int doorID = i;
            DoorIdentifier idComp = holder.GetComponent<DoorIdentifier>() ?? holder.AddComponent<DoorIdentifier>();
            idComp.doorID = doorID;

            SpecialDoorConfig special = specialDoors.Find(d => d.doorIndex == i);

            if (special != null)
            {
                idComp.doorName = special.doorName;

                if (holder.TryGetComponent(out DoorReferences references))
                {
                    idComp.targetSceneName = special.targetSceneName; // TODO: integrate enum (DoorIdentifierName), avoid duplicata
                    references.trigger.targetSceneName = special.targetSceneName; // TODO: remove
                    references.trigger.inProgress = special.inProgress;

                    if (special.doorColor != null)
                        references.controller.SetDoorColor(special.doorColor);

                    if (wallToDisable != null)
                    {
                        references.trigger.wallToDisable = wallToDisable;
                    }
                }
            }
        }

        EditorSceneManager.MarkSceneDirty(carouselTop.gameObject.scene);
        Debug.Log($"Built {doorCount} doors (including {specialDoors.Count} special doors).");
    }
}

// TODO: update door color parametrization to handle specific texture and details

// TODO: auto close the menu on build complete
