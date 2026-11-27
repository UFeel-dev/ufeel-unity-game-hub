using UnityEngine;

namespace GameHub.Maze
{
    public static class MazeTextures
    {
        private const int FloorSize = 512;
        private const int FloorTiles = 4;
        private const int Grout = 4;
        private const int Bevel = 3;
        private const int WallWidth = 256;
        private const int WallHeight = 512;
        private const int CeilingSize = 256;
        private const int Seed = 1337;
        private const int NoiseCell = 8;

        private static readonly Color Slate = new(0.36f, 0.38f, 0.4f);
        private static readonly Color GroutColor = new(0.14f, 0.15f, 0.16f);
        private static readonly Color Plaster = new(0.74f, 0.72f, 0.68f);
        private static readonly Color Skirting = new(0.2f, 0.22f, 0.24f);
        private static readonly Color Rail = new(0.5f, 0.5f, 0.5f);
        private static readonly Color Panel = new(0.58f, 0.6f, 0.62f);
        private static readonly Color PanelLine = new(0.32f, 0.34f, 0.36f);

        public static Texture2D Floor()
        {
            Color32[] pixels = new Color32[FloorSize * FloorSize];
            int tile = FloorSize / FloorTiles;
            for (int y = 0; y < FloorSize; y++)
            {
                for (int x = 0; x < FloorSize; x++)
                {
                    int tx = x % tile;
                    int ty = y % tile;
                    Color color;
                    if (tx < Grout || ty < Grout)
                    {
                        color = GroutColor;
                    }
                    else
                    {
                        float shade = 1f + ((Hash(x / tile, y / tile) - 0.5f) * 0.14f);
                        float grain = 1f + ((Noise(x, y, NoiseCell, FloorSize, FloorSize) - 0.5f) * 0.12f) + ((Hash(x, y) - 0.5f) * 0.05f);
                        color = Slate * shade * grain;
                        if (tx < Grout + Bevel || ty >= tile - Bevel)
                            color *= 1.12f;
                        else if (tx >= tile - Bevel || ty < Grout + Bevel)
                            color *= 0.85f;
                    }

                    pixels[(y * FloorSize) + x] = Opaque(color);
                }
            }

            return Build("MazeFloor", FloorSize, FloorSize, pixels);
        }

        public static Texture2D Wall()
        {
            Color32[] pixels = new Color32[WallWidth * WallHeight];
            for (int y = 0; y < WallHeight; y++)
            {
                float v = (float)y / WallHeight;
                for (int x = 0; x < WallWidth; x++)
                {
                    float grain = 1f + ((Noise(x, y, NoiseCell, WallWidth, WallHeight) - 0.5f) * 0.08f) + ((Hash(x, y) - 0.5f) * 0.03f);
                    Color color = Plaster * grain;
                    if (v < 0.07f)
                        color = Skirting * (v > 0.062f ? 1.6f : grain);
                    else if (v is > 0.34f and < 0.36f)
                        color = Rail * (v > 0.352f ? 1.25f : 0.9f);
                    else if (v > 0.94f)
                        color = Plaster * 0.82f * grain;
                    else if (x < 2)
                        color *= 0.9f;

                    pixels[(y * WallWidth) + x] = Opaque(color);
                }
            }

            return Build("MazeWall", WallWidth, WallHeight, pixels);
        }

        public static Texture2D Ceiling()
        {
            Color32[] pixels = new Color32[CeilingSize * CeilingSize];
            int panel = CeilingSize / 2;
            for (int y = 0; y < CeilingSize; y++)
            {
                for (int x = 0; x < CeilingSize; x++)
                {
                    bool line = x % panel < 3 || y % panel < 3;
                    float grain = 1f + ((Hash(x, y) - 0.5f) * 0.04f);
                    pixels[(y * CeilingSize) + x] = Opaque(line ? PanelLine : Panel * grain);
                }
            }

            return Build("MazeCeiling", CeilingSize, CeilingSize, pixels);
        }

        private static Texture2D Build(string name, int width, int height, Color32[] pixels)
        {
            Texture2D texture = new(width, height, TextureFormat.RGBA32, true)
            {
                name = name,
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Trilinear,
                anisoLevel = 4,
            };
            texture.SetPixels32(pixels);
            texture.Apply(true, true);
            return texture;
        }

        private static Color32 Opaque(Color color)
        {
            color.a = 1f;
            return color;
        }

        private static float Hash(int x, int y)
        {
            unchecked
            {
                uint h = (uint)((x * 374761393) + (y * 668265263) + Seed);
                h = (h ^ (h >> 13)) * 1274126177u;
                return ((h ^ (h >> 16)) & 0xFFFFFF) / (float)0x1000000;
            }
        }

        private static float Noise(int x, int y, int cell, int width, int height)
        {
            int columns = width / cell;
            int rows = height / cell;
            int ix = x / cell;
            int iy = y / cell;
            float u = Mathf.SmoothStep(0f, 1f, (x % cell) / (float)cell);
            float v = Mathf.SmoothStep(0f, 1f, (y % cell) / (float)cell);
            float bottom = Mathf.Lerp(Hash(ix % columns, iy % rows), Hash((ix + 1) % columns, iy % rows), u);
            float top = Mathf.Lerp(Hash(ix % columns, (iy + 1) % rows), Hash((ix + 1) % columns, (iy + 1) % rows), u);
            return Mathf.Lerp(bottom, top, v);
        }
    }
}
