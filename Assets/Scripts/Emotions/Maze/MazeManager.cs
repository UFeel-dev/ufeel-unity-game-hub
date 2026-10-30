using System.Collections;
using System.Collections.Generic;
using UFeel;
using UnityEngine;
using UnityEngine.UI;

public class MazeManager : MonoBehaviour
{
    public static MazeManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject roomPrefab;
    [SerializeField] private GameObject playerObject;
    [SerializeField] private Text emotionDebugText;

    [Header("Maze Settings")]
    public static int MazeSize = 3;
    public float RoomSpacing = 25f;

    private RoomController[,] rooms;

    private int currentX;
    private int currentZ;

    private const float MaxEmotionValue = 3f;
    private static readonly Dictionary<EmotionType, float> _emotionLevels = new();

    private static WaitForSeconds _waitForSeconds5 = new(5f);
    private bool playerDidWin;

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
        UFeelDebugHUD.Clear();

        GenerateRooms();

        foreach (EmotionInfo emotionInfo in EmotionInfo.Emotions)
        {
            _emotionLevels[emotionInfo.Emotion] = 0f;
        }

        InitializeDetection();
    }

    private static void InitializeDetection()
    {
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.V))
        {
            InitializeDetection();
        }

        EmotionType? currentEmotion = UFeelAPI.Emotion.Dominant;

        // Debug.Log($"Current emotion: {currentEmotion}, Text assigned: {emotionDebugText != null}");

        if (currentEmotion == null)
            return;

        foreach (EmotionInfo emotionInfo in EmotionInfo.Emotions)
        {
            EmotionType emotion = emotionInfo.Emotion;

            _emotionLevels[emotion] =
                emotion == currentEmotion
                    ? Mathf.Min(
                        MaxEmotionValue,
                        _emotionLevels[emotion] + Time.deltaTime
                    )
                    : Mathf.Max(
                        0f,
                        _emotionLevels[emotion] - Time.deltaTime
                    );
        }

        UpdateEmotionDebugText(currentEmotion);
    }

    public static bool IsEmotionCharged(EmotionType emotion)
    {
        return _emotionLevels.TryGetValue(emotion, out float value)
            && value >= MaxEmotionValue;
    }

    public static float GetEmotionLevel(EmotionType emotion)
    {
        return _emotionLevels.TryGetValue(emotion, out float value)
            ? value
            : 0f;
    }

    private void UpdateEmotionDebugText(EmotionType? currentEmotion)
    {
        if (emotionDebugText == null)
            return;

        if (playerDidWin)
            return;

        System.Text.StringBuilder builder = new();

        foreach (EmotionInfo emotionInfo in EmotionInfo.Emotions)
        {
            EmotionType emotion = emotionInfo.Emotion;
            float value = GetEmotionLevel(emotion);

            string color = "white";

            if (emotion == currentEmotion)
            {
                color = value >= 2.5f ? "green" : "orange";
            }
            builder.Append(
                $"<color={color}>{emotion} ({value:F1})</color>    "
            );
        }

        emotionDebugText.text = builder.ToString();
    }

    private void GenerateRooms()
    {
        rooms = new RoomController[MazeSize, MazeSize];

        GameObject parent = new("GeneratedRooms");

        for (int x = 0; x < MazeSize; x++)
        {
            for (int z = 0; z < MazeSize; z++)
            {
                Vector3 position = new(
                    x * RoomSpacing,
                    0f,
                    z * RoomSpacing
                );

                GameObject roomObject = Instantiate(roomPrefab, position, Quaternion.identity);

                roomObject.name = $"Room_{x}_{z}";
                roomObject.transform.SetParent(parent.transform);

                RoomController room = roomObject.GetComponent<RoomController>();

                room.Setup(x, z);

                rooms[x, z] = room;
            }
        }

        Debug.Log("All rooms generated.");
    }

    public void TryMove(RoomController room, Direction direction)
    {
        FogCondition condition = room.GetCondition(direction);

        if (playerDidWin)
        {
            return;
        }

        if (!condition.CanPass())
        {
            return;
        }

        int targetX = room.Data.X;
        int targetZ = room.Data.Z;

        switch (direction)
        {
            case Direction.North:
                targetZ++;
                break;

            case Direction.South:
                targetZ--;
                break;

            case Direction.East:
                targetX++;
                break;

            case Direction.West:
                targetX--;
                break;
        }

        if (targetX < 0 ||
            targetX >= MazeSize ||
            targetZ < 0 ||
            targetZ >= MazeSize)
        {
            Debug.Log("Cannot leave labyrinth.");
            return;
        }

        currentX = targetX;
        currentZ = targetZ;

        TeleportPlayer(currentX, currentZ, direction);

        Debug.Log($"Moved to ({currentX},{currentZ})");

        CheckVictory();
    }

    private void TeleportPlayer(int x, int z, Direction cameFrom)
    {
        Vector3 targetPosition = new(
            x * RoomSpacing,
            1f,
            z * RoomSpacing
        );

        switch (cameFrom)
        {
            case Direction.North:
                targetPosition.z -= 5f;
                break;

            case Direction.South:
                targetPosition.z += 5f;
                break;

            case Direction.East:
                targetPosition.x -= 5f;
                break;

            case Direction.West:
                targetPosition.x += 5f;
                break;
        }

        if (playerObject.TryGetComponent(out FirstPersonController firstPersonController))
        {
            firstPersonController.Controller.enabled = false;
            firstPersonController.transform.position = targetPosition;
            firstPersonController.Controller.enabled = true;
        }
    }

    private void CheckVictory()
    {
        if (currentX == MazeSize - 1 && currentZ == MazeSize - 1)
        {
            playerDidWin = true;

            emotionDebugText.text = "<color=green>You won! Congratulations</color>";

            StartCoroutine(VictoryRoutine());
        }
    }

    private static IEnumerator VictoryRoutine()
    {
        yield return _waitForSeconds5;

        PauseMenu.GoToLobby();
    }
}
