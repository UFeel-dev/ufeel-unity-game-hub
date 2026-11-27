using System.Collections.Generic;
using GameHub.Common;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.Maze
{
    public sealed class MazeMenuBackdrop
    {
        private const int Columns = 40;
        private const int Rows = 24;
        private const int CellPixels = 32;
        private const int WallPixels = 3;
        private const float DoorChance = 0.07f;
        private const float DoorLength = 0.6f;
        private const float WallStrength = 0.22f;
        private const float DoorStrength = 0.55f;
        private const float VisibleWidth = 0.8f;
        private static readonly Vector2 Drift = new(0.004f, 0.0025f);
        private static readonly Color Ground = new(0.035f, 0.05f, 0.055f, 1f);
        private static readonly Color WallColor = new(0.55f, 0.68f, 0.68f, 1f);

        private readonly RawImage image;
        private readonly Texture2D texture;
        private Vector2 offset;

        public MazeMenuBackdrop(UiKit kit, RectTransform parent, IReadOnlyList<EmotionInfo> doorColors)
        {
            texture = Paint(doorColors);
            image = UiKit.NewRect("Backdrop", parent).gameObject.AddComponent<RawImage>();
            image.texture = texture;
            image.raycastTarget = false;
            UiLayout.Stretch(image.rectTransform);

            Image vignette = UiKit.NewImage("Vignette", parent, kit.Sprites.Vignette, Theme.Backdrop);
            UiLayout.Stretch(vignette.rectTransform);
        }

        public void Tick(float deltaTime)
        {
            Rect rect = image.rectTransform.rect;
            float height = rect.width > 0f ? VisibleWidth * rect.height / rect.width * texture.width / texture.height : VisibleWidth;
            offset += Drift * deltaTime;
            offset.x %= 1f;
            offset.y %= 1f;
            image.uvRect = new Rect(offset, new Vector2(VisibleWidth, height));
        }

        public void Release()
        {
            Object.Destroy(texture);
        }

        private static Texture2D Paint(IReadOnlyList<EmotionInfo> doorColors)
        {
            bool[,] east = new bool[Columns, Rows];
            bool[,] north = new bool[Columns, Rows];
            Carve(east, north);

            int width = Columns * CellPixels;
            int height = Rows * CellPixels;
            Color32[] pixels = new Color32[width * height];
            Color32 ground = Ground;
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = ground;

            Color32 wall = Color.Lerp(Ground, WallColor, WallStrength);
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    int left = x * CellPixels;
                    int bottom = y * CellPixels;
                    DrawEdge(pixels, width, height, left + CellPixels, bottom, false, east[x, y], wall, doorColors);
                    DrawEdge(pixels, width, height, left, bottom + CellPixels, true, north[x, y], wall, doorColors);
                }
            }

            Texture2D result = new(width, height, TextureFormat.RGBA32, false)
            {
                name = "MazeMenuBackdrop",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear,
            };
            result.SetPixels32(pixels);
            result.Apply(false, true);
            return result;
        }

        private static void DrawEdge(Color32[] pixels, int width, int height, int x, int y, bool horizontal, bool open, Color32 wall, IReadOnlyList<EmotionInfo> doorColors)
        {
            int start = 0;
            int length = CellPixels + WallPixels;
            Color32 color = wall;
            if (open)
            {
                if (doorColors.Count == 0 || Random.value >= DoorChance)
                    return;

                Color emotion = doorColors[Random.Range(0, doorColors.Count)].Color;
                color = Color.Lerp(Ground, emotion, DoorStrength);
                length = Mathf.RoundToInt(CellPixels * DoorLength);
                start = (CellPixels - length) / 2;
            }

            for (int along = start; along < start + length; along++)
            {
                for (int across = 0; across < WallPixels; across++)
                {
                    int px = horizontal ? x + along : x + across;
                    int py = horizontal ? y + across : y + along;
                    pixels[(Mod(py, height) * width) + Mod(px, width)] = color;
                }
            }
        }

        private static void Carve(bool[,] east, bool[,] north)
        {
            bool[,] visited = new bool[Columns, Rows];
            Stack<Vector2Int> stack = new();
            List<Direction> open = new(4);
            stack.Push(Vector2Int.zero);
            visited[0, 0] = true;
            while (stack.Count > 0)
            {
                Vector2Int current = stack.Peek();
                open.Clear();
                foreach (Direction direction in MazeGrid.Directions)
                {
                    Vector2Int next = Wrap(current + MazeGrid.OffsetOf(direction));
                    if (!visited[next.x, next.y])
                        open.Add(direction);
                }

                if (open.Count == 0)
                {
                    stack.Pop();
                    continue;
                }

                Direction side = open[Random.Range(0, open.Count)];
                Vector2Int target = Wrap(current + MazeGrid.OffsetOf(side));
                switch (side)
                {
                    case Direction.East:
                        east[current.x, current.y] = true;
                        break;
                    case Direction.West:
                        east[target.x, target.y] = true;
                        break;
                    case Direction.North:
                        north[current.x, current.y] = true;
                        break;
                    default:
                        north[target.x, target.y] = true;
                        break;
                }

                visited[target.x, target.y] = true;
                stack.Push(target);
            }
        }

        private static Vector2Int Wrap(Vector2Int cell) => new(Mod(cell.x, Columns), Mod(cell.y, Rows));

        private static int Mod(int value, int modulus) => ((value % modulus) + modulus) % modulus;
    }
}
