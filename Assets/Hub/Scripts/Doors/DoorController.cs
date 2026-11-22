using TMPro;
using UnityEngine;

namespace GameHub.Hub
{
    public class DoorController : MonoBehaviour
    {
        private const float LabelOffset = 0.005f;
        private const float LabelHeight = 0.4f;
        private const float LabelRaise = 0.5f;
        private const float LabelWidthRatio = 0.9f;

        [SerializeField] private GameObject door;

        public Color DoorColor
        {
            get
            {
                if (!door.TryGetComponent(out MeshRenderer meshRenderer) || meshRenderer.sharedMaterial == null)
                {
                    Debug.LogWarning($"{door.name} has no material, its color defaults to white.", this);
                    return Color.white;
                }

                return meshRenderer.sharedMaterial.color;
            }
        }

        public void ToggleDoor()
        {
            if (door.TryGetComponent(out Door doorComponent))
                doorComponent.Toggle();
        }

        public void SetDoorColor(Color color)
        {
            if (!door.TryGetComponent(out MeshRenderer meshRenderer) || meshRenderer.sharedMaterial == null)
                return;

            meshRenderer.sharedMaterial = new Material(meshRenderer.sharedMaterial) { color = color };
        }

        public void ShowStatusLabel(string text)
        {
            if (!door.TryGetComponent(out MeshFilter meshFilter) || meshFilter.sharedMesh == null)
            {
                Debug.LogWarning($"No mesh on {door.name}, cannot place the '{text}' label.", this);
                return;
            }

            Bounds bounds = meshFilter.sharedMesh.bounds;
            Vector3 extents = bounds.extents;
            bool depthIsX = extents.x < extents.z;
            Vector3 normal = depthIsX ? Vector3.right : Vector3.forward;
            float depth = (depthIsX ? extents.x : extents.z) + LabelOffset;
            float width = (depthIsX ? extents.z : extents.x) * 2f * LabelWidthRatio;
            Vector2 size = new(width, LabelHeight);
            Vector3 center = bounds.center + (Vector3.up * extents.y * LabelRaise);

            CreateLabel(text, door.transform, center - (normal * depth), Quaternion.LookRotation(normal, Vector3.up), size);
            CreateLabel(text, door.transform, center + (normal * depth), Quaternion.LookRotation(-normal, Vector3.up), size);
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
    }
}
