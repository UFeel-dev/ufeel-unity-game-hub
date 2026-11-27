using UnityEngine;

namespace GameHub.Maze
{
    public sealed class DoorHighlight
    {
        private const string ShaderName = "Sprites/Default";
        private const float Width = 0.12f;
        private const float ChargedWidth = 0.22f;
        private const float FaceOffset = 0.05f;
        private const float PulseSpeed = 4f;
        private const float MinBrightness = 0.55f;

        private readonly LineRenderer line;
        private readonly Material material;
        private readonly Vector3[] corners = new Vector3[4];

        public DoorHighlight(Transform owner)
        {
            GameObject frame = new("DoorHighlight");
            frame.transform.SetParent(owner, false);
            material = new Material(Shader.Find(ShaderName));
            line = frame.AddComponent<LineRenderer>();
            line.sharedMaterial = material;
            line.useWorldSpace = true;
            line.loop = false;
            line.positionCount = corners.Length;
            line.numCornerVertices = 4;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.enabled = false;
        }

        public void Show(Transform door, Vector3 roomCenter, Color color, float progress)
        {
            Vector3 front = door.TransformPoint(new Vector3(0f, 0f, 0.5f));
            Vector3 back = door.TransformPoint(new Vector3(0f, 0f, -0.5f));
            float face = (front - roomCenter).sqrMagnitude < (back - roomCenter).sqrMagnitude ? 0.5f : -0.5f;
            Vector3 outward = (face > 0f ? front - back : back - front).normalized * FaceOffset;

            corners[0] = door.TransformPoint(new Vector3(-0.5f, -0.5f, face)) + outward;
            corners[1] = door.TransformPoint(new Vector3(-0.5f, 0.5f, face)) + outward;
            corners[2] = door.TransformPoint(new Vector3(0.5f, 0.5f, face)) + outward;
            corners[3] = door.TransformPoint(new Vector3(0.5f, -0.5f, face)) + outward;
            line.SetPositions(corners);

            float pulse = Mathf.Lerp(MinBrightness, 1f, (Mathf.Sin(Time.time * PulseSpeed) + 1f) / 2f);
            Color glow = Color.Lerp(color, Color.white, progress * 0.5f) * pulse;
            glow.a = 1f;
            line.startColor = glow;
            line.endColor = glow;
            line.widthMultiplier = Mathf.Lerp(Width, ChargedWidth, progress);
            line.enabled = true;
        }

        public void Hide()
        {
            line.enabled = false;
        }

        public void Release()
        {
            if (line != null)
                Object.Destroy(line.gameObject);
            Object.Destroy(material);
        }
    }
}
