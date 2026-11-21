using DoorScript;
using TMPro;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private GameObject _door;

    private const float LabelOffset = 0.005f;
    private const float LabelHeight = 0.4f;
    private const float LabelRaise = 0.5f;

    private void Awake()
    {
        if (!_door.TryGetComponent<MeshRenderer>(out _))
        {
            Debug.LogWarning("MeshRenderer not found on DoorController GameObject!");
        }
    }

    public void ToggleDoor()
    {
        if (_door.TryGetComponent(out Door doorScript))
        {
            doorScript.ToggleDoor();
        }
    }

    public void SetDoorColor(Color color)
    {
        if (_door.TryGetComponent(out MeshRenderer meshRenderer))
        {
            if (meshRenderer == null) return;
            if (meshRenderer.sharedMaterial == null) return;

            meshRenderer.sharedMaterial = new Material(meshRenderer.sharedMaterial)
            {
                color = color
            };
        }
    }

    public void ShowStatusLabel(string text)
    {
        if (!_door.TryGetComponent(out MeshFilter meshFilter) || meshFilter.sharedMesh == null)
        {
            Debug.LogWarning($"No mesh on {_door.name}, cannot place the '{text}' label.");
            return;
        }

        Bounds bounds = meshFilter.sharedMesh.bounds;
        Vector3 extents = bounds.extents;
        bool depthIsX = extents.x < extents.z;
        Vector3 normal = depthIsX ? Vector3.right : Vector3.forward;
        float depth = (depthIsX ? extents.x : extents.z) + LabelOffset;
        float width = (depthIsX ? extents.z : extents.x) * 2f * 0.9f;
        Vector2 size = new(width, LabelHeight);
        Vector3 center = bounds.center + (Vector3.up * extents.y * LabelRaise);

        CreateLabel(text, _door.transform, center - (normal * depth), Quaternion.LookRotation(normal, Vector3.up), size);
        CreateLabel(text, _door.transform, center + (normal * depth), Quaternion.LookRotation(-normal, Vector3.up), size);
    }

    private static void CreateLabel(string text, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector2 size)
    {
        GameObject label = new($"StatusLabel_{text}", typeof(RectTransform));
        label.transform.SetParent(parent, false);
        label.transform.SetLocalPositionAndRotation(localPosition, localRotation);

        TextMeshPro mesh = label.AddComponent<TextMeshPro>();
        mesh.text = text;
        mesh.alignment = TextAlignmentOptions.Center;
        mesh.enableAutoSizing = true;
        mesh.fontSizeMin = 0.1f;
        mesh.fontSizeMax = 2f;
        mesh.color = Color.red;
        mesh.outlineWidth = 0.2f;
        mesh.outlineColor = Color.black;
        mesh.rectTransform.sizeDelta = size;
    }

    public Color DoorColor
    {
        get
        {
            if (!_door.TryGetComponent(out MeshRenderer meshRenderer))
            {
                Debug.LogWarning("MeshRenderer not found on Door GameObject!");
                return Color.white;
            }

            if (meshRenderer.sharedMaterial == null)
            {
                Debug.LogWarning("Door material is null!");
                return Color.white;
            }

            return meshRenderer.sharedMaterial.color;
        }
    }
}

// TODO: update to directly retrieve the door from the attached object
