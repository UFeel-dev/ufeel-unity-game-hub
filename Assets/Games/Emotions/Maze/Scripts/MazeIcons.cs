using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameHub.Maze
{
    public sealed class MazeIcons
    {
        private const int Resolution = 128;
        private const float RimWidth = 0.06f;
        private const float ShadowWidth = 0.2f;
        private const float ShadowOpacity = 0.55f;
        private const float ConeHalfAngle = 38f;
        private const float ConeOpacity = 0.45f;
        private const float ConeEdge = 6f;

        private static readonly Vector2[] TriangleRoomShape =
        {
            new(-0.96f, -0.96f),
            new(0.96f, -0.96f),
            new(0f, 0.96f),
        };

        private static readonly Vector2[] ArrowShape =
        {
            new(0f, 0.7f),
            new(0.48f, -0.56f),
            new(0f, -0.3f),
            new(-0.48f, -0.56f),
        };

        private static readonly Vector2[] StarShape =
        {
            new(0f, 0.7f), new(0.16f, 0.16f), new(0.7f, 0f), new(0.16f, -0.16f),
            new(0f, -0.7f), new(-0.16f, -0.16f), new(-0.7f, 0f), new(-0.16f, 0.16f),
        };

        private static readonly Color32 RimColor = new(5, 8, 9, 255);

        private readonly List<UnityEngine.Object> owned = new();

        public MazeIcons()
        {
            Exit = Create(p => Mathf.Min(Ring(p, 0.72f, 0.11f), Circle(p, 0.3f)));
            Hint = Create(p => Mathf.Min(Polygon(p, StarShape), Ring(p, 0.86f, 0.05f)));
            Bonus = Create(Grid);
            Trap = Create(p => Mathf.Min(Bar(p, new Vector2(-0.62f, -0.62f), new Vector2(0.62f, 0.62f), 0.12f), Bar(p, new Vector2(-0.62f, 0.62f), new Vector2(0.62f, -0.62f), 0.12f)));
            Start = Create(p => Ring(p, 0.6f, 0.13f));
            TriangleRoom = Create(p => Polygon(p, TriangleRoomShape));
            Padlock = Create(Lock);
            Check = Create(p => Mathf.Min(Bar(p, new Vector2(-0.6f, 0f), new Vector2(-0.18f, -0.42f), 0.13f), Bar(p, new Vector2(-0.18f, -0.42f), new Vector2(0.62f, 0.45f), 0.13f)));
            Player = CreateMarker(p => Polygon(p, ArrowShape) - 0.08f);
            ViewCone = CreateCone();
        }

        public Sprite Exit { get; }

        public Sprite Hint { get; }

        public Sprite Bonus { get; }

        public Sprite Trap { get; }

        public Sprite Start { get; }

        public Sprite TriangleRoom { get; }

        public Sprite Padlock { get; }

        public Sprite Check { get; }

        public Sprite Player { get; }

        public Sprite ViewCone { get; }

        public Sprite? Of(RoomKind kind)
        {
            return kind switch
            {
                RoomKind.Exit => Exit,
                RoomKind.Hint => Hint,
                RoomKind.Bonus => Bonus,
                RoomKind.Trap => Trap,
                RoomKind.Start => Start,
                _ => null,
            };
        }

        public void Release()
        {
            foreach (UnityEngine.Object item in owned)
                UnityEngine.Object.Destroy(item);
            owned.Clear();
        }

        private static float Circle(Vector2 p, float radius) => p.magnitude - radius;

        private static float Ring(Vector2 p, float radius, float halfWidth) => Mathf.Abs(p.magnitude - radius) - halfWidth;

        private static float Bar(Vector2 p, Vector2 a, Vector2 b, float halfWidth)
        {
            Vector2 ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
            return (p - (a + (ab * t))).magnitude - halfWidth;
        }

        private static float Box(Vector2 p, Vector2 center, Vector2 half, float radius)
        {
            Vector2 d = new(Mathf.Abs(p.x - center.x) - half.x + radius, Mathf.Abs(p.y - center.y) - half.y + radius);
            return new Vector2(Mathf.Max(d.x, 0f), Mathf.Max(d.y, 0f)).magnitude + Mathf.Min(Mathf.Max(d.x, d.y), 0f) - radius;
        }

        private static float Grid(Vector2 p)
        {
            const float Offset = 0.36f;
            Vector2 half = new(0.28f, 0.28f);
            return Mathf.Min(
                Mathf.Min(Box(p, new Vector2(-Offset, -Offset), half, 0.08f), Box(p, new Vector2(Offset, -Offset), half, 0.08f)),
                Mathf.Min(Box(p, new Vector2(-Offset, Offset), half, 0.08f), Box(p, new Vector2(Offset, Offset), half, 0.08f)));
        }

        private static float Lock(Vector2 p)
        {
            float body = Box(p, new Vector2(0f, -0.28f), new Vector2(0.56f, 0.44f), 0.12f);
            float keyhole = Mathf.Min(Circle(p - new Vector2(0f, -0.2f), 0.11f), Bar(p, new Vector2(0f, -0.2f), new Vector2(0f, -0.45f), 0.05f));
            Vector2 arch = p - new Vector2(0f, 0.2f);
            float shackle = arch.y >= 0f ? Ring(arch, 0.34f, 0.09f) : Mathf.Abs(Mathf.Abs(arch.x) - 0.34f) - 0.09f;
            return Mathf.Min(Mathf.Max(body, -keyhole), Mathf.Max(shackle, -(p.y - 0.12f)));
        }

        private static float Polygon(Vector2 point, Vector2[] vertices)
        {
            float distance = (point - vertices[0]).sqrMagnitude;
            bool inside = false;
            int previous = vertices.Length - 1;
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 edge = vertices[previous] - vertices[i];
                Vector2 offset = point - vertices[i];
                Vector2 closest = offset - (edge * Mathf.Clamp01(Vector2.Dot(offset, edge) / Vector2.Dot(edge, edge)));
                distance = Mathf.Min(distance, closest.sqrMagnitude);
                bool above = point.y >= vertices[i].y;
                bool below = point.y < vertices[previous].y;
                bool left = edge.x * offset.y > edge.y * offset.x;
                if (above == below && below == left)
                    inside = !inside;
                previous = i;
            }

            return (inside ? -1f : 1f) * Mathf.Sqrt(distance);
        }

        private static Vector2 PointAt(int x, int y)
        {
            return new Vector2((((x + 0.5f) / Resolution) * 2f) - 1f, (((y + 0.5f) / Resolution) * 2f) - 1f);
        }

        private static float Coverage(float distance) => Mathf.Clamp01(0.5f - (distance * Resolution / 2f));

        private Sprite Create(Func<Vector2, float> shape)
        {
            return Paint(p => new Color(1f, 1f, 1f, Coverage(shape(p))));
        }

        private Sprite CreateMarker(Func<Vector2, float> shape)
        {
            return Paint(p =>
            {
                float distance = shape(p);
                float fill = Coverage(distance + RimWidth);
                float rim = Coverage(distance);
                float shadow = ShadowOpacity * Mathf.Clamp01(1f - (distance / ShadowWidth));
                Color color = Color.Lerp(RimColor, Color.white, fill);
                color.a = Mathf.Max(rim, distance > 0f ? shadow * shadow : 0f);
                return color;
            });
        }

        private Sprite CreateCone()
        {
            return Paint(p =>
            {
                float radius = p.magnitude;
                float angle = Mathf.Abs(Mathf.Atan2(p.x, p.y) * Mathf.Rad2Deg);
                float edge = Mathf.Clamp01((ConeHalfAngle - angle) / ConeEdge);
                float fade = Mathf.Clamp01(1f - radius);
                return new Color(1f, 1f, 1f, ConeOpacity * edge * fade);
            });
        }

        private Sprite Paint(Func<Vector2, Color> shade)
        {
            Color32[] pixels = new Color32[Resolution * Resolution];
            for (int y = 0; y < Resolution; y++)
            {
                for (int x = 0; x < Resolution; x++)
                    pixels[(y * Resolution) + x] = shade(PointAt(x, y));
            }

            Texture2D texture = new(Resolution, Resolution, TextureFormat.RGBA32, true)
            {
                name = "MazeIcon",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Trilinear,
            };
            texture.SetPixels32(pixels);
            texture.Apply(true, true);

            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, Resolution, Resolution), new Vector2(0.5f, 0.5f), Resolution);
            owned.Add(texture);
            owned.Add(sprite);
            return sprite;
        }
    }
}
