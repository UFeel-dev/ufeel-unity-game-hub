using System.Collections;
using System.Collections.Generic;
using GameHub.Common;
using UFeel;
using UnityEngine;

namespace GameHub.Maze
{
    public class MazeManager : MonoBehaviour
    {
        private const float MaxEmotionLevel = 3f;

        private const float DecayRate = 0.35f;
        private const float PassEmotionLevel = 2f;
        private const float PlayerHeight = 1f;
        private const float RelockDistance = 3f;
        private const float ResultDelay = 4f;
        private const int BonusRevealRadius = 2;
        private const int HintRouteLength = 3;

        [Header("References")]
        [SerializeField] private GameObject roomPrefab;
        [SerializeField] private GameObject playerObject;
        [Tooltip("Levels in play order and Unlimited mode rules.")]
        [SerializeField] private MazeCatalog catalog;

        [Header("Layout")]
        [Tooltip("Must match the room prefab's floor size so neighbouring rooms touch.")]
        [SerializeField] private float roomSpacing = 20f;

        private readonly Dictionary<EmotionType, float> emotionLevels = new();
        private readonly List<EmotionInfo> emotions = new();
        private readonly HashSet<Vector2Int> usedBonuses = new();
        private FirstPersonController player;
        private MazeLevelMenu? menu;
        private MazeRun run;
        private MazeLevelDefinition? level;
        private MazeGrid? grid;
        private RoomController[,] rooms;
        private GameObject? roomsRoot;
        private MazeHud hud;
        private MazeIcons? icons;
        private MazeLook? look;
        private DoorHighlight? highlight;
        private Vector2Int playerRoom;
        private Direction? entrySide;
        private bool hintRevealed;
        private bool playerWon;

        private void Start()
        {
            player = playerObject.GetComponent<FirstPersonController>();
            icons = new MazeIcons();
            look = roomPrefab.GetComponent<RoomController>().CreateLook();
            OpenMenu();
        }

        private void OnDestroy()
        {
            menu?.Close();
            EndRun();
            icons?.Release();
            look?.Release();
        }

        private void Update()
        {
            if (menu != null)
            {
                UpdateMenu();
                return;
            }

            if (grid == null || level == null)
                return;

            hud.Tick(Time.deltaTime);
            if (UiFocus.IsFree && LayoutInput.GetKeyDown(KeyCode.L))
            {
                EndRun();
                OpenMenu();
                return;
            }

            if (playerWon)
                return;

            UpdateLevels();

            Vector3 position = player.transform.position;
            hud.Map.Track(new Vector2(position.x / roomSpacing, position.z / roomSpacing), player.transform.eulerAngles.y);
            Vector2Int room = RoomAt(position);
            if (room != playerRoom)
                EnterRoom(room);

            if (playerWon)
                return;

            UpdateDoors();
            RelockPassages();
        }

        private void OpenMenu()
        {
            player.enabled = false;
            menu = new MazeLevelMenu(UiKit.Shared, transform, catalog, icons);
        }

        private void UpdateMenu()
        {
            int? row = menu!.Tick(Time.deltaTime);
            if (row == null)
                return;

            if (row.Value == menu.HubRow)
                GameFlow.ReturnToHub();
            else if (row.Value == menu.UnlimitedRow)
                StartRun(MazeRun.Unlimited(0));
            else
                StartRun(MazeRun.Level(row.Value));
        }

        private void StartRun(MazeRun chosen)
        {
            menu?.Close();
            menu = null;
            EndRun();
            run = chosen;
            level = chosen.IsUnlimited ? catalog.Unlimited : catalog.Levels[chosen.LevelIndex];
            int size = chosen.IsUnlimited ? catalog.UnlimitedSize(chosen.Round) : level.Size;

            player.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            emotions.AddRange(catalog.EmotionsOf(level));
            grid = MazeGrid.Generate(level, size, emotions);
            highlight = new DoorHighlight(transform);
            hud = new MazeHud(UiKit.Shared, transform, emotions, grid, icons);
            foreach (EmotionInfo info in emotions)
                emotionLevels[info.Emotion] = 0f;

            GenerateRooms();
            player.Teleport(RoomCenter(grid.Start));
            playerRoom = grid.Start;
            EnterRoom(grid.Start);
            if (chosen.IsUnlimited)
                hud.ShowIntro("Unlimited", $"Maze {chosen.Round + 1}");
            else
                hud.ShowIntro($"Level {chosen.LevelIndex + 1}", "Find the exit");
        }

        private void EndRun()
        {
            if (grid == null)
                return;

            StopAllCoroutines();
            if (roomsRoot != null)
            {
                roomsRoot.SetActive(false);
                Destroy(roomsRoot);
            }

            hud.Destroy();
            highlight?.Release();
            highlight = null;
            grid = null;
            level = null;
            emotionLevels.Clear();
            emotions.Clear();
            usedBonuses.Clear();
            entrySide = null;
            hintRevealed = false;
            playerWon = false;
        }

        private void SetEmotionLevel(EmotionType emotion, float value)
        {
            emotionLevels[emotion] = value;
            hud.SetLevel(emotion, value / PassEmotionLevel, emotion == UFeelAPI.Emotion.Dominant);
        }

        private void ResetLevels()
        {
            foreach (EmotionInfo info in emotions)
                SetEmotionLevel(info.Emotion, 0f);
        }

        private void UpdateLevels()
        {
            EmotionType? dominant = UFeelAPI.Emotion.Dominant;
            if (dominant == null)
                return;

            foreach (EmotionInfo info in emotions)
            {
                float value = emotionLevels[info.Emotion];
                emotionLevels[info.Emotion] = info.Emotion == dominant
                    ? Mathf.Min(MaxEmotionLevel, value + Time.deltaTime)
                    : Mathf.Max(0f, value - (DecayRate * Time.deltaTime));
                hud.SetLevel(info.Emotion, emotionLevels[info.Emotion] / PassEmotionLevel, info.Emotion == dominant);
            }
        }

        private void UpdateDoors()
        {
            RoomController room = RoomOf(playerRoom);
            Vector3 position = player.transform.position;
            Passage? target = null;
            float closest = float.MaxValue;
            foreach (Direction direction in MazeGrid.Directions)
            {
                Passage? passage = grid!.PassageAt(playerRoom, direction);
                if (passage == null)
                    continue;

                room.DoorOf(direction).SetProgress(0f);
                if (passage.IsOpen || !passage.CanTraverseFrom(playerRoom))
                    continue;

                Vector3 door = room.DoorOf(direction).transform.position;
                float distance = new Vector2(door.x - position.x, door.z - position.z).sqrMagnitude;
                if (distance < closest)
                {
                    closest = distance;
                    target = passage;
                }
            }

            if (target == null)
            {
                highlight!.Hide();
            }
            else
            {
                DoorFog door = room.DoorOf(target.SideFrom(playerRoom));
                float progress = Mathf.Clamp01(emotionLevels[target.Emotion.Emotion] / PassEmotionLevel);
                door.SetProgress(progress);
                highlight!.Show(door.transform, room.transform.position, target.Emotion.Color, progress);
                if (progress >= 1f)
                {
                    target.Open(playerRoom);
                    ShowPassage(target);
                    SetEmotionLevel(target.Emotion.Emotion, 0f);
                }
            }

            if (!hintRevealed && grid!.TryGetHint(playerRoom, out EmotionInfo hint) && emotionLevels[hint.Emotion] >= PassEmotionLevel)
                RevealHint();
        }

        private void RevealHint()
        {
            hintRevealed = true;
            Direction direction = grid!.DirectionToExit(playerRoom);
            RoomOf(playerRoom).DoorOf(direction).SetHint(true);

            List<Vector2Int> path = grid.PathToExit(playerRoom);
            List<Vector2Int> route = path.GetRange(0, Mathf.Min(HintRouteLength, path.Count));
            route.Add(grid.Exit);
            hud.Map.ShowRoute(route);

            EmotionType doorEmotion = grid.PassageAt(playerRoom, direction)!.Emotion.Emotion;
            hud.Notify(direction == entrySide
                ? $"The way out is behind you: back through the {doorEmotion} door"
                : $"The way out is through the {doorEmotion} door");
        }

        private void RelockPassages()
        {
            Vector3 position = player.transform.position;
            foreach (Passage passage in grid!.Passages)
            {
                if (!passage.IsOpen || !passage.RelocksBehind || playerRoom == passage.OpenedFrom)
                    continue;

                Vector3 doorway = (RoomCenter(passage.From) + RoomCenter(passage.To)) / 2f;
                if (Vector2.Distance(new Vector2(position.x, position.z), new Vector2(doorway.x, doorway.z)) <= RelockDistance)
                    continue;

                passage.Close();
                ShowPassage(passage);
            }
        }

        private void EnterRoom(Vector2Int room)
        {
            RoomController previous = RoomOf(playerRoom);
            foreach (Direction direction in MazeGrid.Directions)
            {
                previous.DoorOf(direction).SetProgress(0f);
                previous.DoorOf(direction).SetHint(false);
            }

            Vector2Int offset = playerRoom - room;
            entrySide = null;
            foreach (Direction direction in MazeGrid.Directions)
            {
                if (MazeGrid.OffsetOf(direction) == offset && grid!.PassageAt(room, direction) != null)
                    entrySide = direction;
            }

            playerRoom = room;
            hintRevealed = false;
            ResetLevels();
            hud.Map.Visit(room);

            switch (grid!.KindOf(room))
            {
                case RoomKind.Exit:
                    Win();
                    break;
                case RoomKind.Trap:
                    RoomOf(room).ShowKind(RoomKind.Trap);
                    hud.Notify("A trap! Back to the start");
                    player.Teleport(RoomCenter(grid.Start));
                    EnterRoom(grid.Start);
                    break;
                case RoomKind.Bonus when usedBonuses.Add(room):
                    hud.Map.Reveal(room, BonusRevealRadius);
                    hud.Notify("Bonus room: the map around you is revealed");
                    break;
                case RoomKind.Hint:
                    grid.TryGetHint(room, out EmotionInfo hint);
                    hud.Notify($"Hint room: feel {hint.Emotion} to reveal the way out");
                    break;
            }
        }

        private void ShowPassage(Passage passage)
        {
            RoomOf(passage.From).DoorOf(passage.Side).SetOpen(passage.IsOpen);
            RoomOf(passage.To).DoorOf(MazeGrid.Opposite(passage.Side)).SetOpen(passage.IsOpen);
        }

        private void GenerateRooms()
        {
            roomsRoot = new GameObject("GeneratedRooms");
            Transform parent = roomsRoot.transform;
            rooms = new RoomController[grid!.Size, grid.Size];

            for (int x = 0; x < grid.Size; x++)
            {
                for (int z = 0; z < grid.Size; z++)
                {
                    GameObject roomObject = Instantiate(roomPrefab, new Vector3(x * roomSpacing, 0f, z * roomSpacing), Quaternion.identity, parent);
                    roomObject.name = $"Room_{x}_{z}";
                    RoomController room = roomObject.GetComponent<RoomController>();
                    room.Setup(grid, new Vector2Int(x, z), icons!, look!);
                    rooms[x, z] = room;
                }
            }
        }

        private RoomController RoomOf(Vector2Int room)
        {
            return rooms[room.x, room.y];
        }

        private Vector2Int RoomAt(Vector3 position)
        {
            int x = Mathf.Clamp(Mathf.RoundToInt(position.x / roomSpacing), 0, grid!.Size - 1);
            int z = Mathf.Clamp(Mathf.RoundToInt(position.z / roomSpacing), 0, grid.Size - 1);
            return new Vector2Int(x, z);
        }

        private Vector3 RoomCenter(Vector2Int room)
        {
            return new Vector3(room.x * roomSpacing, PlayerHeight, room.y * roomSpacing);
        }

        private void Win()
        {
            playerWon = true;
            highlight!.Hide();
            MazeRun? next;
            if (run.IsUnlimited)
            {
                MazeSave.RecordUnlimited(run.Round + 1);
                next = MazeRun.Unlimited(run.Round + 1);
                hud.ShowResult($"Maze {run.Round + 1} cleared", $"Next: maze {run.Round + 2}");
            }
            else
            {
                int number = run.LevelIndex + 1;
                MazeSave.CompleteLevel(run.LevelIndex);
                bool last = number >= catalog.Levels.Count;
                next = last ? null : MazeRun.Level(number);
                hud.ShowResult(
                    $"Level {number} complete",
                    last ? "Every level is cleared: Unlimited mode is unlocked" : $"Next: Level {number + 1}");
            }

            StartCoroutine(ContinueAfterDelay(next));
        }

        private IEnumerator ContinueAfterDelay(MazeRun? next)
        {
            yield return new WaitForSeconds(ResultDelay);
            if (next.HasValue)
            {
                StartRun(next.Value);
            }
            else
            {
                EndRun();
                OpenMenu();
            }
        }
    }
}
