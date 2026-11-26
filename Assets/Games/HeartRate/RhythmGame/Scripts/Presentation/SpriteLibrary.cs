using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameHub.RhythmGame
{
    public sealed class SpriteLibrary
    {
        public const float PillBorder = 31f;

        private const int AtlasSize = 512;
        private const float PixelsPerUnit = 100f;
        private const int PillSize = 64;
        private const float ArrowRounding = 6f;
        private const float OutlineThickness = 9f;

        private static readonly RectInt VignetteArea = new(0, 0, 256, 256);
        private static readonly RectInt CircleArea = new(264, 0, 128, 128);
        private static readonly RectInt RingArea = new(264, 136, 128, 128);
        private static readonly RectInt GlowArea = new(264, 272, 128, 128);
        private static readonly RectInt PillArea = new(400, 0, PillSize, PillSize);
        private static readonly RectInt FadeArea = new(400, 72, 16, 64);
        private static readonly RectInt SolidArea = new(424, 72, 8, 8);
        private static readonly RectInt HeartArea = new(400, 144, 64, 64);
        private static readonly RectInt TriangleArea = new(400, 216, 32, 32);
        private static readonly RectInt ArrowArea = new(0, 264, 128, 128);
        private static readonly RectInt ArrowOutlineArea = new(136, 264, 128, 128);

        private static readonly Vector2[] ArrowShape =
        {
            new(64f, 114f), new(114f, 64f), new(83f, 64f), new(83f, 16f), new(45f, 16f), new(45f, 64f), new(14f, 64f),
        };

        private const int Supersample = 4;

        private readonly Texture2D atlas;
        private readonly List<Sprite> sprites = new();

        public SpriteLibrary()
        {
            Color32[] pixels = new Color32[AtlasSize * AtlasSize];
            Paint(pixels, VignetteArea, VignetteAlpha);
            Paint(pixels, CircleArea, CircleAlpha);
            Paint(pixels, RingArea, RingAlpha);
            Paint(pixels, GlowArea, GlowAlpha);
            Paint(pixels, PillArea, CircleAlpha);
            Paint(pixels, FadeArea, FadeAlpha);
            Paint(pixels, SolidArea, (_, _, _) => 1f);
            PaintShape(pixels, HeartArea, InHeart);
            PaintShape(pixels, TriangleArea, InTriangle);
            PaintArrow(pixels, ArrowArea, 0f);
            PaintArrow(pixels, ArrowOutlineArea, OutlineThickness);

            atlas = new Texture2D(AtlasSize, AtlasSize, TextureFormat.RGBA32, false)
            {
                name = "RhythmGameAtlas",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            atlas.SetPixels32(pixels);
            atlas.Apply(false, true);

            Vignette = Create(VignetteArea, Vector4.zero);
            Circle = Create(CircleArea, Vector4.zero);
            Ring = Create(RingArea, Vector4.zero);
            Glow = Create(GlowArea, Vector4.zero);
            Pill = Create(PillArea, new Vector4(PillBorder, PillBorder, PillBorder, PillBorder));
            Fade = Create(FadeArea, Vector4.zero);
            Solid = Create(new RectInt(SolidArea.x + 2, SolidArea.y + 2, SolidArea.width - 4, SolidArea.height - 4), Vector4.zero);
            Heart = Create(HeartArea, Vector4.zero);
            Triangle = Create(TriangleArea, Vector4.zero);
            Arrow = Create(ArrowArea, Vector4.zero);
            ArrowOutline = Create(ArrowOutlineArea, Vector4.zero);
        }

        public Sprite Vignette { get; }

        public Sprite Circle { get; }

        public Sprite Ring { get; }

        public Sprite Glow { get; }

        public Sprite Pill { get; }

        public Sprite Fade { get; }

        public Sprite Solid { get; }

        public Sprite Heart { get; }

        public Sprite Triangle { get; }

        public Sprite Arrow { get; }

        public Sprite ArrowOutline { get; }

        public void Release()
        {
            foreach (Sprite sprite in sprites)
                UnityEngine.Object.Destroy(sprite);
            sprites.Clear();
            UnityEngine.Object.Destroy(atlas);
        }

        private static void Paint(Color32[] pixels, RectInt area, Func<float, float, float, float> alpha)
        {
            float radius = Mathf.Min(area.width, area.height) / 2f;
            for (int y = 0; y < area.height; y++)
            {
                for (int x = 0; x < area.width; x++)
                {
                    float dx = x + 0.5f - (area.width / 2f);
                    float dy = y + 0.5f - (area.height / 2f);
                    float value = Mathf.Clamp01(alpha(Mathf.Sqrt((dx * dx) + (dy * dy)), radius, (y + 0.5f) / area.height));
                    pixels[((area.y + y) * AtlasSize) + area.x + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(value * 255f));
                }
            }
        }

        private static void PaintShape(Color32[] pixels, RectInt area, Func<float, float, bool> inside)
        {
            for (int y = 0; y < area.height; y++)
            {
                for (int x = 0; x < area.width; x++)
                {
                    int covered = 0;
                    for (int sy = 0; sy < Supersample; sy++)
                    {
                        for (int sx = 0; sx < Supersample; sx++)
                        {
                            float u = (x + ((sx + 0.5f) / Supersample)) / area.width;
                            float v = (y + ((sy + 0.5f) / Supersample)) / area.height;
                            if (inside(u, v))
                                covered++;
                        }
                    }

                    byte alpha = (byte)(covered * 255 / (Supersample * Supersample));
                    pixels[((area.y + y) * AtlasSize) + area.x + x] = new Color32(255, 255, 255, alpha);
                }
            }
        }

        private static void PaintArrow(Color32[] pixels, RectInt area, float outline)
        {
            for (int y = 0; y < area.height; y++)
            {
                for (int x = 0; x < area.width; x++)
                {
                    float distance = PolygonDistance(new Vector2(x + 0.5f, y + 0.5f), ArrowShape) - ArrowRounding;
                    float value = Mathf.Clamp01(0.5f - distance);
                    if (outline > 0f)
                        value *= Mathf.Clamp01(distance + outline + 0.5f);
                    pixels[((area.y + y) * AtlasSize) + area.x + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(value * 255f));
                }
            }
        }

        private static float PolygonDistance(Vector2 point, Vector2[] vertices)
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

        private static bool InHeart(float u, float v)
        {
            float x = (u - 0.5f) * 2.6f;
            float y = ((v - 0.5f) * 2.6f) + 0.15f;
            float a = (x * x) + (y * y) - 1f;
            return (a * a * a) - (x * x * y * y * y) <= 0f;
        }

        private static bool InTriangle(float u, float v)
        {
            float half = 0.4f * (1f - ((v - 0.15f) / 0.7f));
            return v >= 0.15f && v <= 0.85f && Mathf.Abs(u - 0.5f) <= half;
        }

        private static float CircleAlpha(float distance, float radius, float height)
        {
            return radius - 1f - distance + 0.5f;
        }

        private static float RingAlpha(float distance, float radius, float height)
        {
            const float thickness = 9f;
            float outer = radius - 1f;
            return Mathf.Clamp01(outer - distance + 0.5f) * Mathf.Clamp01(distance - (outer - thickness) + 0.5f);
        }

        private static float GlowAlpha(float distance, float radius, float height)
        {
            float normalized = distance / (radius - 2f);
            return Mathf.Exp(-normalized * normalized * 3.5f) * Mathf.Clamp01((radius - distance) / 4f);
        }

        private static float VignetteAlpha(float distance, float radius, float height)
        {
            return Mathf.SmoothStep(0f, 1f, ((distance / radius) - 0.55f) / 0.85f);
        }

        private static float FadeAlpha(float distance, float radius, float height)
        {
            return 1f - height;
        }

        private Sprite Create(RectInt area, Vector4 border)
        {
            Rect rect = new(area.x, area.y, area.width, area.height);
            Sprite sprite = Sprite.Create(atlas, rect, new Vector2(0.5f, 0.5f), PixelsPerUnit, 0, SpriteMeshType.FullRect, border);
            sprite.name = atlas.name;
            sprites.Add(sprite);
            return sprite;
        }
    }
}
