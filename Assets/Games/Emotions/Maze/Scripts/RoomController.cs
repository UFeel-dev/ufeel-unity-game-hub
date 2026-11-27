using System.Collections.Generic;
using GameHub.Common;
using UnityEngine;
using UnityEngine.Rendering;

namespace GameHub.Maze
{
    public class RoomController : MonoBehaviour
    {
        private const float DoorHalfWidth = 2f;
        private const float WallThickness = 0.5f;

        private const float OpeningMargin = 0.2f;
        private const float CorridorOverlap = 0.3f;
        private const float MinWallLength = 0.05f;

        private const float CornerColumn = 0.8f;
        private const float JambColumn = 0.6f;
        private const float JambLead = 0.08f;
        private const float JambInset = 0.05f;
        private const float JambOffset = DoorHalfWidth + (JambColumn / 2f) + JambInset;

        private const float FloorTint = 0.2f;
        private const float EmblemSize = 5f;
        private const float EmblemLift = 0.02f;
        private const float EmblemOpacity = 0.9f;
        private const float LampDiameter = 1.6f;
        private const float LampThickness = 0.06f;

        private static readonly Color ColumnTint = new(0.62f, 0.62f, 0.64f, 1f);

        [Header("Doors")]
        [SerializeField] private DoorFog northDoor;
        [SerializeField] private DoorFog southDoor;
        [SerializeField] private DoorFog eastDoor;
        [SerializeField] private DoorFog westDoor;

        [Header("Walls")]
        [SerializeField] private Transform northWall;
        [SerializeField] private Transform southWall;
        [SerializeField] private Transform eastWall;
        [SerializeField] private Transform westWall;

        [Header("Floor and ceiling")]
        [SerializeField] private Renderer floor;
        [SerializeField] private Renderer roof;

        [Header("Lamp")]
        [SerializeField] private Color lampColor = new(1f, 0.92f, 0.8f, 1f);
        [SerializeField, Min(0f)] private float lampIntensity = 1.6f;
        [SerializeField, Min(0f)] private float lampRange = 14f;

        private MazeIcons icons;
        private MazeLook look;
        private Renderer? emblem;

        public MazeLook CreateLook()
        {
            return new MazeLook(floor.sharedMaterial, northWall.GetComponent<Renderer>().sharedMaterial, roof.sharedMaterial);
        }

        public void Setup(MazeGrid grid, Vector2Int coordinates, MazeIcons iconSet, MazeLook roomLook)
        {
            icons = iconSet;
            look = roomLook;
            look.Paint(roof, look.Ceiling, Tiling(roof.transform.localScale.x, roof.transform.localScale.z, MazeLook.CeilingTile), Color.white);

            List<Direction> doorways = new(MazeGrid.Directions.Length);
            foreach (Direction direction in MazeGrid.Directions)
            {
                Passage? passage = grid.PassageAt(coordinates, direction);
                DoorFog door = DoorOf(direction);
                if (passage == null)
                {
                    door.gameObject.SetActive(false);
                    continue;
                }

                doorways.Add(direction);
                door.Setup(passage.Emotion, passage.Kind, passage.CanTraverseFrom(coordinates));
            }

            BuildWalls(grid.LayoutOf(coordinates), doorways);
            AddLamp();

            RoomKind kind = grid.KindOf(coordinates);
            ShowKind(kind == RoomKind.Trap ? RoomKind.Normal : kind);
        }

        public void ShowKind(RoomKind kind)
        {
            Color? tint = RoomKindStyle.ColorOf(kind);
            Color floorTint = tint.HasValue ? Color.Lerp(Color.white, tint.Value, FloorTint) : Color.white;
            look.Paint(floor, look.Floor, Tiling(floor.transform.localScale.x, floor.transform.localScale.z, MazeLook.FloorTile), floorTint);

            Sprite? icon = icons.Of(kind);
            if (icon == null || !tint.HasValue)
            {
                if (emblem != null)
                    emblem.gameObject.SetActive(false);
                return;
            }

            emblem ??= CreateEmblem();
            emblem.gameObject.SetActive(true);
            emblem.material.mainTexture = icon.texture;
            emblem.material.color = Theme.Fade(tint.Value, EmblemOpacity);
        }

        public DoorFog DoorOf(Direction direction)
        {
            return direction switch
            {
                Direction.North => northDoor,
                Direction.South => southDoor,
                Direction.East => eastDoor,
                _ => westDoor,
            };
        }

        private static Vector2 Tiling(float width, float height, float tile)
        {
            return new Vector2(width / tile, height / tile);
        }

        private static float AngleOf(Vector2 along)
        {
            return Mathf.Atan2(-along.y, along.x) * Mathf.Rad2Deg;
        }

        private void OnDestroy()
        {
            if (emblem != null)
                Destroy(emblem.material);
        }

        private float FloorTop => floor.transform.localPosition.y + (floor.transform.localScale.y / 2f);

        private float CeilingBottom => roof.transform.localPosition.y - (roof.transform.localScale.y / 2f);

        private Renderer CreateEmblem()
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "Emblem";
            Destroy(quad.GetComponent<Collider>());

            quad.transform.SetParent(transform, false);
            quad.transform.localPosition = new Vector3(floor.transform.localPosition.x, FloorTop + EmblemLift, floor.transform.localPosition.z);
            quad.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            quad.transform.localScale = new Vector3(EmblemSize, EmblemSize, 1f);

            Renderer renderer = quad.GetComponent<Renderer>();
            renderer.sharedMaterial = northDoor.GetComponent<Renderer>().sharedMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            return renderer;
        }

        private void AddLamp()
        {
            GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = "Lamp";
            Destroy(disc.GetComponent<Collider>());
            disc.transform.SetParent(transform, false);
            disc.transform.localPosition = new Vector3(0f, CeilingBottom - (LampThickness / 2f), 0f);
            disc.transform.localScale = new Vector3(LampDiameter, LampThickness / 2f, LampDiameter);
            Renderer renderer = disc.GetComponent<Renderer>();
            renderer.sharedMaterial = look.Lamp;
            renderer.shadowCastingMode = ShadowCastingMode.Off;

            Light lamp = new GameObject("LampLight").AddComponent<Light>();
            lamp.transform.SetParent(transform, false);
            lamp.transform.localPosition = new Vector3(0f, CeilingBottom - 0.6f, 0f);
            lamp.type = LightType.Point;
            lamp.color = lampColor;
            lamp.intensity = lampIntensity;
            lamp.range = lampRange;
            lamp.shadows = LightShadows.None;
        }

        private void BuildWalls(RoomLayout layout, List<Direction> doorways)
        {
            Transform template = northWall;
            foreach (Transform wall in new[] { northWall, southWall, eastWall, westWall })
                wall.gameObject.SetActive(false);

            List<Vector2> outline = RoomGeometry.Outline(layout);
            List<(Vector2 A, Vector2 B)> segments = new(outline.Count);
            for (int i = 0; i < outline.Count; i++)
            {
                Vector2 a = outline[i];
                Vector2 b = outline[(i + 1) % outline.Count];
                Vector2 extension = (b - a).normalized * (WallThickness / 2f);
                segments.Add((a - extension, b + extension));
                if (layout.Shape != RoomShape.Circle)
                    AddBlock(template, a, AngleOf(b - a), CornerColumn, CornerColumn, ColumnTint);
            }

            foreach (Direction direction in doorways)
                RoomGeometry.CutDoorway(segments, direction, DoorHalfWidth + OpeningMargin);
            foreach ((Vector2 a, Vector2 b) in segments)
                AddWall(template, a, b);

            foreach (Direction direction in doorways)
            {
                Vector2 forward = RoomGeometry.Forward(direction);
                Vector2 side = RoomGeometry.Lateral(direction);
                foreach (float sign in new[] { 1f, -1f })
                {
                    Vector2 lane = side * (sign * (DoorHalfWidth + (WallThickness / 2f)));
                    float start = RoomGeometry.DistanceToOutline(outline, lane, forward) - CorridorOverlap;
                    AddWall(template, lane + (forward * start), lane + (forward * RoomGeometry.CellHalf));

                    Vector2 jamb = side * (sign * JambOffset);
                    float jambDistance = start - JambLead + (JambColumn / 2f);
                    AddBlock(template, jamb + (forward * jambDistance), AngleOf(forward), JambColumn, JambColumn, ColumnTint);
                }
            }
        }

        private void AddWall(Transform template, Vector2 a, Vector2 b)
        {
            Vector2 along = b - a;
            float length = along.magnitude;
            if (length >= MinWallLength)
                AddBlock(template, (a + b) / 2f, AngleOf(along), length, WallThickness, Color.white);
        }

        private void AddBlock(Transform template, Vector2 center, float angle, float length, float depth, Color tint)
        {
            Transform block = Instantiate(template, transform);
            block.name = "Wall";
            block.gameObject.SetActive(true);
            block.localPosition = new Vector3(center.x, template.localPosition.y, center.y);
            block.localRotation = Quaternion.Euler(0f, angle, 0f);
            block.localScale = new Vector3(length, template.localScale.y, depth);
            look.Paint(block.GetComponent<Renderer>(), look.Wall, new Vector2(length / MazeLook.WallTile, 1f), tint);
        }
    }
}
