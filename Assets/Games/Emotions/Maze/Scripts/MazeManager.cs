using System.Collections;
using System.Collections.Generic;
using System.Text;
using GameHub.Common;
using UFeel;
using UnityEngine;
using UnityEngine.UI;

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
        [SerializeField] private Text emotionDebugText;

        [Header("Layout")]
        [SerializeField] private float roomSpacing = 25f;

        private readonly Dictionary<EmotionType, float> emotionLevels = new();
        private Vector2Int playerRoom;
        private bool playerWon;

        public static MazeManager? Instance { get; private set; }

        private void Awake()
        {
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
                emotionLevels[info.Emotion] = 0f;

            GenerateRooms();
        }

        private void Update()
        {
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

            UpdateEmotionText(dominant.Value);
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

        private void UpdateEmotionText(EmotionType dominant)
        {
            if (emotionDebugText == null || playerWon)
                return;

            StringBuilder builder = new();
            foreach (EmotionInfo info in EmotionInfo.All)
            {
                float level = emotionLevels[info.Emotion];
                string color = info.Emotion != dominant ? "white" : level >= PassEmotionLevel ? "green" : "orange";
                builder.Append($"<color={color}>{info.Emotion} ({level:F1})</color>    ");
            }

            emotionDebugText.text = builder.ToString();
        }

        private void Win()
        {
            playerWon = true;

            if (emotionDebugText != null)
                emotionDebugText.text = "<color=green>You won! Congratulations</color>";

            StartCoroutine(ReturnToHubAfterDelay());
        }

        private static IEnumerator ReturnToHubAfterDelay()
        {
            yield return new WaitForSeconds(VictoryDelay);
            GameFlow.ReturnToHub();
        }
    }
}
