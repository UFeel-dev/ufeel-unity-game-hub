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
        private const float PassEmotionLevel = 2f;
        private const float PlayerHeight = 1f;
        private const float EntryDistanceFromCenter = 5f;
        private const float VictoryDelay = 5f;

        [Header("References")]
        [SerializeField] private GameObject roomPrefab;
        [SerializeField] private GameObject playerObject;

        [Header("Layout")]
        [SerializeField] private float roomSpacing = 25f;

        private readonly Dictionary<EmotionType, float> emotionLevels = new();
        private MazeHud hud;
        private Vector2Int playerRoom;
        private bool playerWon;

        public static MazeManager? Instance { get; private set; }

        private void Awake()
        {
            hud = new MazeHud(UiKit.Shared, transform, EmotionInfo.All);
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            foreach (EmotionInfo info in EmotionInfo.All)
            {
                emotionLevels[info.Emotion] = 0f;
                hud.SetLevel(info.Emotion, 0f, false);
            }

            GenerateRooms();
        }

        private void Update()
        {
            hud.Tick(Time.deltaTime);
            EmotionType? dominant = UFeelAPI.Emotion.Dominant;
            if (dominant == null)
                return;

            foreach (EmotionInfo info in EmotionInfo.All)
            {
                float level = emotionLevels[info.Emotion];
                emotionLevels[info.Emotion] = info.Emotion == dominant
                    ? Mathf.Min(MaxEmotionLevel, level + Time.deltaTime)
                    : Mathf.Max(0f, level - Time.deltaTime);
            }

            UpdateHud(dominant.Value);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void TryMove(RoomController room, Direction direction)
        {
            if (playerWon || emotionLevels[room.GetDoorEmotion(direction)] < PassEmotionLevel)
                return;

            Vector2Int target = room.Coordinates + MazeGrid.OffsetOf(direction);
            if (!MazeGrid.IsInside(target))
                return;

            playerRoom = target;
            TeleportPlayer(direction);

            if (playerRoom == MazeGrid.Exit)
                Win();
        }

        private void GenerateRooms()
        {
            Transform parent = new GameObject("GeneratedRooms").transform;

            for (int x = 0; x < MazeGrid.Size; x++)
            {
                for (int z = 0; z < MazeGrid.Size; z++)
                {
                    GameObject roomObject = Instantiate(roomPrefab, new Vector3(x * roomSpacing, 0f, z * roomSpacing), Quaternion.identity, parent);
                    roomObject.name = $"Room_{x}_{z}";
                    roomObject.GetComponent<RoomController>().Setup(new Vector2Int(x, z));
                }
            }
        }

        private void TeleportPlayer(Direction movedTowards)
        {
            Vector2Int offset = MazeGrid.OffsetOf(movedTowards);
            Vector3 roomCenter = new(playerRoom.x * roomSpacing, PlayerHeight, playerRoom.y * roomSpacing);
            Vector3 entry = roomCenter - (new Vector3(offset.x, 0f, offset.y) * EntryDistanceFromCenter);

            if (playerObject.TryGetComponent(out FirstPersonController controller))
                controller.Teleport(entry);
        }

        private void UpdateHud(EmotionType dominant)
        {
            if (playerWon)
                return;

            foreach (EmotionInfo info in EmotionInfo.All)
                hud.SetLevel(info.Emotion, emotionLevels[info.Emotion] / PassEmotionLevel, info.Emotion == dominant);
        }

        private void Win()
        {
            playerWon = true;

            hud.ShowWin();

            StartCoroutine(ReturnToHubAfterDelay());
        }

        private static IEnumerator ReturnToHubAfterDelay()
        {
            yield return new WaitForSeconds(VictoryDelay);
            GameFlow.ReturnToHub();
        }
    }
}
