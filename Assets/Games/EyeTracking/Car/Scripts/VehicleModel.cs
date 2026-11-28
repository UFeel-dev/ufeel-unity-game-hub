using UnityEngine;

namespace GameHub.Car
{
    public sealed class VehicleModel
    {
        private const float WheelRadius = 0.3f;
        private const float WheelWidth = 0.24f;
        private const float Clearance = 0.2f;
        private const float BodyHeight = 0.42f;
        private const float CabinHeight = 0.38f;
        private const float MaxSteerAngle = 25f;
        private const string SmoothnessProperty = "_Smoothness";
        private const string EmissionProperty = "_EmissionColor";
        private const string EmissionKeyword = "_EMISSION";

        private static readonly Color Paint = new(0.1f, 0.34f, 0.7f);
        private static readonly Color Glass = new(0.08f, 0.11f, 0.15f);
        private static readonly Color Rubber = new(0.06f, 0.06f, 0.07f);
        private static readonly Color Trim = new(0.18f, 0.19f, 0.21f);
        private static readonly Color Headlight = new(1f, 0.93f, 0.75f);
        private static readonly Color Taillight = new(0.9f, 0.08f, 0.06f);

        private readonly Transform[] frontWheels = new Transform[2];

        public VehicleModel(Transform vehicle)
        {
            MeshRenderer box = vehicle.GetComponent<MeshRenderer>();
            Material source = box.sharedMaterial;
            box.enabled = false;

            Vector3 size = vehicle.lossyScale;
            Transform root = new GameObject("Model").transform;
            root.SetParent(vehicle, false);
            root.localScale = new Vector3(1f / size.x, 1f / size.y, 1f / size.z);

            float ground = -size.y / 2f;
            float width = size.x;
            float length = size.z;
            float bodyTop = ground + Clearance + BodyHeight;

            Material paint = Tint(source, Paint, 0.7f);
            Material glass = Tint(source, Glass, 0.9f);
            Material rubber = Tint(source, Rubber, 0.1f);
            Material trim = Tint(source, Trim, 0.3f);

            Part(root, PrimitiveType.Cube, paint, new Vector3(0f, ground + Clearance + (BodyHeight / 2f), 0f), new Vector3(width, BodyHeight, length));
            Part(root, PrimitiveType.Cube, glass, new Vector3(0f, bodyTop + (CabinHeight / 2f), -length * 0.08f), new Vector3(width * 0.82f, CabinHeight, length * 0.45f));
            Part(root, PrimitiveType.Cube, paint, new Vector3(0f, bodyTop + CabinHeight, -length * 0.08f), new Vector3(width * 0.86f, 0.06f, length * 0.47f));

            float bumperY = ground + Clearance + 0.06f;
            Part(root, PrimitiveType.Cube, trim, new Vector3(0f, bumperY, length / 2f), new Vector3(width * 1.02f, 0.12f, 0.12f));
            Part(root, PrimitiveType.Cube, trim, new Vector3(0f, bumperY, -length / 2f), new Vector3(width * 1.02f, 0.12f, 0.12f));

            float lightY = ground + Clearance + (BodyHeight * 0.65f);
            Vector3 lightSize = new(0.28f, 0.1f, 0.04f);
            Material front = Glow(source, Headlight);
            Material back = Glow(source, Taillight);
            for (int side = -1; side <= 1; side += 2)
            {
                Part(root, PrimitiveType.Cube, front, new Vector3(side * width * 0.32f, lightY, (length / 2f) + 0.01f), lightSize);
                Part(root, PrimitiveType.Cube, back, new Vector3(side * width * 0.32f, lightY, (-length / 2f) - 0.01f), lightSize);
            }

            int index = 0;
            for (int axle = -1; axle <= 1; axle += 2)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    Transform pivot = new GameObject("Wheel").transform;
                    pivot.SetParent(root, false);
                    pivot.localPosition = new Vector3(side * ((width / 2f) - 0.04f), ground + WheelRadius, axle * length * 0.32f);
                    Transform wheel = Part(pivot, PrimitiveType.Cylinder, rubber, Vector3.zero, new Vector3(WheelRadius * 2f, WheelWidth / 2f, WheelRadius * 2f));
                    wheel.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    if (axle > 0)
                        frontWheels[index++] = pivot;
                }
            }
        }

        public void Steer(float amount)
        {
            Quaternion rotation = Quaternion.Euler(0f, Mathf.Clamp(amount, -1f, 1f) * MaxSteerAngle, 0f);
            foreach (Transform wheel in frontWheels)
                wheel.localRotation = rotation;
        }

        private static Transform Part(Transform parent, PrimitiveType type, Material material, Vector3 position, Vector3 scale)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            Object.DestroyImmediate(part.GetComponent<Collider>());
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.GetComponent<MeshRenderer>().sharedMaterial = material;
            return part.transform;
        }

        private static Material Tint(Material source, Color color, float smoothness)
        {
            Material material = new(source) { color = color };
            material.SetFloat(SmoothnessProperty, smoothness);
            return material;
        }

        private static Material Glow(Material source, Color color)
        {
            Material material = Tint(source, color, 0.5f);
            material.EnableKeyword(EmissionKeyword);
            material.SetColor(EmissionProperty, color * 1.5f);
            return material;
        }
    }
}
