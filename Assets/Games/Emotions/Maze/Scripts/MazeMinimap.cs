using System.Collections.Generic;
using GameHub.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.Maze
{
    public sealed class MazeMinimap
    {
        private const float MapSize = 300f;
        private const float Padding = 16f;
        private const float CellRatio = 0.7f;
        private const float TriangleIconRatio = 0.4f;
        private const float LinkRatio = 0.14f;
        private const float IconRatio = 0.62f;
        private const float MarkerRatio = 0.7f;
        private const float ConeRatio = 2.4f;
        private const float BackgroundOpacity = 0.6f;
        private const float BackgroundRadius = 16f;
        private const float CellRadius = 6f;
        private const float RouteOpacity = 0.45f;
        private const float LegendGap = 10f;
        private const float LegendRow = 32f;
        private const float LegendIcon = 20f;
        private const float LegendTextOffset = 40f;

        private static readonly RoomKind[] LegendKinds = { RoomKind.Exit, RoomKind.Hint, RoomKind.Bonus, RoomKind.Trap, RoomKind.Start };
        private static readonly Color VisitedColor = Theme.Fade(Theme.TextDim, 0.7f);
        private static readonly Color MappedColor = Theme.Fade(Theme.TextFaint, 0.6f);
        private static readonly Color GlimpsedColor = Theme.Fade(Theme.TextFaint, 0.3f);

        private readonly MazeGrid grid;
        private readonly RectTransform root;
        private readonly Knowledge[,] knowledge;
        private readonly Image[,] cells;
        private readonly Image?[,] icons;
        private readonly Dictionary<Passage, Image> links = new();
        private readonly HashSet<Vector2Int> route = new();
        private readonly Dictionary<RoomKind, RectTransform> legendRows = new();
        private readonly RectTransform legend;
        private readonly RectTransform marker;
        private readonly RectTransform cone;
        private readonly float pitch;

        public MazeMinimap(UiKit kit, Transform parent, MazeGrid grid, MazeIcons iconSet)
        {
            this.grid = grid;
            pitch = MapSize / grid.Size;
            knowledge = new Knowledge[grid.Size, grid.Size];
            cells = new Image[grid.Size, grid.Size];
            icons = new Image?[grid.Size, grid.Size];

            root = UiKit.NewRect("Minimap", parent);
            float side = MapSize + (2f * Padding);
            UiLayout.Place(root, Vector2.up, Vector2.up, new Vector2(Theme.Margin, -Theme.Margin), new Vector2(side, side));
            UiLayout.Stretch(kit.NewRounded("Background", root, Theme.Fade(Theme.Backdrop, BackgroundOpacity), BackgroundRadius).rectTransform);

            float cellSize = pitch * CellRatio;
            float linkWidth = pitch * LinkRatio;
            foreach (Passage passage in grid.Passages)
            {
                Vector2 forward = RoomGeometry.Forward(passage.Side);
                Vector2 start = CenterOf(passage.From) + (forward * ExtentOf(passage.From, passage.Side, cellSize));
                Vector2 end = CenterOf(passage.To) - (forward * ExtentOf(passage.To, MazeGrid.Opposite(passage.Side), cellSize));
                float length = Vector2.Distance(start, end);
                Image link = kit.NewSolid("Link", root, passage.Emotion.Color);
                bool horizontal = passage.Side is Direction.East or Direction.West;
                Place(link.rectTransform, (start + end) / 2f, horizontal ? new Vector2(length, linkWidth) : new Vector2(linkWidth, length));
                link.gameObject.SetActive(false);
                links[passage] = link;
            }

            for (int x = 0; x < grid.Size; x++)
            {
                for (int y = 0; y < grid.Size; y++)
                {
                    Vector2Int room = new(x, y);
                    RoomLayout layout = grid.LayoutOf(room);
                    Image cell = NewCell(kit, iconSet, room, cellSize);
                    cell.gameObject.SetActive(false);
                    cells[x, y] = cell;

                    RoomKind kind = grid.KindOf(room);
                    Sprite? sprite = iconSet.Of(kind);
                    if (sprite == null)
                        continue;

                    bool triangle = layout.Shape == RoomShape.Triangle;
                    Vector2 offset = triangle ? RoomGeometry.Forward(layout.Orientation) * (-cellSize / 6f) : Vector2.zero;
                    float iconSize = cellSize * (triangle ? TriangleIconRatio : IconRatio);
                    Image icon = UiKit.NewImage("Icon", root, sprite, RoomKindStyle.ColorOf(kind) ?? Theme.Text);
                    Place(icon.rectTransform, CenterOf(room) + offset, new Vector2(iconSize, iconSize));
                    icon.gameObject.SetActive(false);
                    icons[x, y] = icon;
                }
            }

            float markerSize = cellSize * MarkerRatio;
            cone = UiKit.NewImage("ViewCone", root, iconSet.ViewCone, Color.white).rectTransform;
            Place(cone, CenterOf(grid.Start), new Vector2(markerSize * ConeRatio, markerSize * ConeRatio));
            marker = UiKit.NewImage("Player", root, iconSet.Player, Color.white).rectTransform;
            Place(marker, CenterOf(grid.Start), new Vector2(markerSize, markerSize));

            legend = UiKit.NewRect("Legend", root);
            UiLayout.Stretch(kit.NewRounded("Background", legend, Theme.Fade(Theme.Backdrop, BackgroundOpacity), BackgroundRadius).rectTransform);
            foreach (RoomKind kind in LegendKinds)
            {
                RectTransform row = UiKit.NewRect(kind.ToString(), legend);
                Color color = RoomKindStyle.ColorOf(kind) ?? Theme.Text;
                Image icon = UiKit.NewImage("Icon", row, iconSet.Of(kind)!, color);
                UiLayout.Place(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(Padding, 0f), new Vector2(LegendIcon, LegendIcon));
                TextMeshProUGUI label = kit.NewText("Label", row, Theme.Small, false, TextAlignmentOptions.Left, Theme.TextDim);
                label.text = RoomKindStyle.LabelOf(kind);
                UiLayout.Stretch(label.rectTransform);
                label.rectTransform.offsetMin = new Vector2(LegendTextOffset, 0f);
                row.gameObject.SetActive(false);
                legendRows[kind] = row;
            }
        }

        private enum Knowledge
        {
            Hidden,
            Glimpsed,
            Mapped,
            Visited,
        }

        public void SetVisible(bool visible)
        {
            root.gameObject.SetActive(visible);
        }

        public void Visit(Vector2Int room)
        {
            Raise(room, Knowledge.Visited);
            foreach (Direction direction in MazeGrid.Directions)
            {
                Passage? passage = grid.PassageAt(room, direction);
                if (passage != null)
                    Raise(passage.Other(room), Knowledge.Glimpsed);
            }

            Refresh();
        }

        public void Track(Vector2 position, float yaw)
        {
            marker.anchoredPosition = new Vector2(Padding + ((position.x + 0.5f) * pitch), Padding + ((position.y + 0.5f) * pitch));
            marker.localRotation = Quaternion.Euler(0f, 0f, -yaw);
            cone.anchoredPosition = marker.anchoredPosition;
            cone.localRotation = marker.localRotation;
        }

        public void ShowRoute(IEnumerable<Vector2Int> rooms)
        {
            foreach (Vector2Int room in rooms)
            {
                Raise(room, Knowledge.Mapped);
                route.Add(room);
            }

            Refresh();
        }

        public void Reveal(Vector2Int center, int radius)
        {
            for (int x = center.x - radius; x <= center.x + radius; x++)
            {
                for (int y = center.y - radius; y <= center.y + radius; y++)
                {
                    Vector2Int room = new(x, y);
                    if (grid.IsInside(room))
                        Raise(room, Knowledge.Mapped);
                }
            }

            Refresh();
        }

        private static void Place(RectTransform rect, Vector2 center, Vector2 size)
        {
            UiLayout.Place(rect, Vector2.zero, new Vector2(0.5f, 0.5f), center, size);
        }

        private static float AngleOf(Direction direction)
        {
            return direction switch
            {
                Direction.East => -90f,
                Direction.South => 180f,
                Direction.West => 90f,
                _ => 0f,
            };
        }

        private Image NewCell(UiKit kit, MazeIcons iconSet, Vector2Int room, float cellSize)
        {
            RoomLayout layout = grid.LayoutOf(room);
            Vector2 size = new(cellSize, cellSize);
            Image cell;
            switch (layout.Shape)
            {
                case RoomShape.Circle:
                    cell = UiKit.NewImage("Cell", root, kit.Sprites.Circle, GlimpsedColor);
                    break;
                case RoomShape.Triangle:
                    cell = UiKit.NewImage("Cell", root, iconSet.TriangleRoom, GlimpsedColor);
                    break;
                case RoomShape.Rectangle:
                    cell = kit.NewRounded("Cell", root, GlimpsedColor, CellRadius);
                    float narrow = cellSize * RoomGeometry.NarrowHalf / RoomGeometry.WallHalf;
                    size = layout.Orientation is Direction.North or Direction.South ? new Vector2(narrow, cellSize) : new Vector2(cellSize, narrow);
                    break;
                default:
                    cell = kit.NewRounded("Cell", root, GlimpsedColor, CellRadius);
                    break;
            }

            Place(cell.rectTransform, CenterOf(room), size);
            if (layout.Shape == RoomShape.Triangle)
                cell.rectTransform.localRotation = Quaternion.Euler(0f, 0f, AngleOf(layout.Orientation));
            return cell;
        }

        private float ExtentOf(Vector2Int room, Direction direction, float cellSize)
        {
            List<Vector2> outline = RoomGeometry.Outline(grid.LayoutOf(room));
            float distance = RoomGeometry.DistanceToOutline(outline, Vector2.zero, RoomGeometry.Forward(direction));
            return cellSize / 2f * distance / RoomGeometry.WallHalf;
        }

        private Vector2 CenterOf(Vector2Int room)
        {
            return new Vector2(Padding + ((room.x + 0.5f) * pitch), Padding + ((room.y + 0.5f) * pitch));
        }

        private Knowledge KnowledgeOf(Vector2Int room)
        {
            return knowledge[room.x, room.y];
        }

        private void Raise(Vector2Int room, Knowledge level)
        {
            if (knowledge[room.x, room.y] < level)
                knowledge[room.x, room.y] = level;
        }

        private bool IsKindKnown(Vector2Int room)
        {
            Knowledge level = KnowledgeOf(room);
            return level >= Knowledge.Mapped || (level == Knowledge.Glimpsed && grid.KindOf(room) == RoomKind.Exit);
        }

        private void Refresh()
        {
            HashSet<RoomKind> shownKinds = new();
            for (int x = 0; x < grid.Size; x++)
            {
                for (int y = 0; y < grid.Size; y++)
                {
                    Vector2Int room = new(x, y);
                    Knowledge level = knowledge[x, y];
                    Image cell = cells[x, y];
                    cell.gameObject.SetActive(level != Knowledge.Hidden);
                    cell.color = ColorOf(room, level);

                    Image? icon = icons[x, y];
                    bool kindShown = icon != null && IsKindKnown(room);
                    icon?.gameObject.SetActive(kindShown);
                    if (kindShown)
                        shownKinds.Add(grid.KindOf(room));
                }
            }

            foreach ((Passage passage, Image link) in links)
                link.gameObject.SetActive(KnowledgeOf(passage.From) >= Knowledge.Mapped || KnowledgeOf(passage.To) >= Knowledge.Mapped);

            RefreshLegend(shownKinds);
        }

        private void RefreshLegend(HashSet<RoomKind> shownKinds)
        {
            int rows = 0;
            foreach (RoomKind kind in LegendKinds)
            {
                RectTransform row = legendRows[kind];
                bool shown = shownKinds.Contains(kind);
                row.gameObject.SetActive(shown);
                if (!shown)
                    continue;

                UiLayout.Row(row, (Padding / 2f) + (rows * LegendRow), LegendRow);
                rows++;
            }

            legend.gameObject.SetActive(rows > 0);
            UiLayout.Place(legend, Vector2.zero, Vector2.up, new Vector2(0f, -LegendGap), new Vector2(root.sizeDelta.x, (rows * LegendRow) + Padding));
        }

        private Color ColorOf(Vector2Int room, Knowledge level)
        {
            if (level != Knowledge.Visited && route.Contains(room))
                return Theme.Fade(RoomKindStyle.ColorOf(RoomKind.Hint)!.Value, RouteOpacity);

            return level switch
            {
                Knowledge.Visited => VisitedColor,
                Knowledge.Mapped => MappedColor,
                _ => GlimpsedColor,
            };
        }
    }
}
