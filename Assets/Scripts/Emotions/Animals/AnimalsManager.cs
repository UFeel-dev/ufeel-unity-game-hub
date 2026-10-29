using System.Collections;
using System.Collections.Generic;
using TMPro;
using UFeel;
using UnityEngine;
using UnityEngine.UI;

public enum AnimalType
{
    Cat,
    Deer,
    Tiger,
    Penguin,
    Spider
}

public class AnimalsManager : MonoBehaviour
{
    private static readonly WaitForSeconds _waitForSeconds1_5 = new(1.5f);
    private static readonly WaitForSeconds _waitForSeconds2 = new(2f);
    private static readonly WaitForSeconds _waitForSeconds5 = new(5f);

    [Header("Scene References")]
    public Transform SpawnPoint;
    public GameObject[] AnimalPrefabs;
    public TextMeshProUGUI InstructionText;
    public TextMeshProUGUI TimerText;
    public TextMeshProUGUI ScoreText;

    [Header("Game Settings")]
    private int currentLevel = 1;
    private float timer;
    private int score;
    private bool isLevelActive;

    private List<AnimalType> animalsList = new();
    private GameObject currentAnimalGO;
    private readonly Dictionary<AnimalType, EmotionType> animalEmotions =
        new()
    {
        { AnimalType.Cat, EmotionType.Happy },
        { AnimalType.Deer, EmotionType.Surprised },
        { AnimalType.Tiger, EmotionType.Angry },
        { AnimalType.Penguin, EmotionType.Sad },
        { AnimalType.Spider, EmotionType.Fearful }
    };

    public Slider EmotionProgressBar;
    private Image barFillImage;

    [Header("Progress Settings")]
    private float currentProgress = 0f;
    private const float MaxProgressDuration = 2.5f;
    private bool isTrackingEmotion = false;
    private AnimalType currentTargetAnimal;

    private void Start()
    {
        UFeelDebugHUD.Clear();

        InitializeDetection();

        if (EmotionProgressBar != null)
        {
            barFillImage = EmotionProgressBar.fillRect.GetComponent<Image>();
        }

        StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop()
    {
        while (currentLevel <= 5)
        {
            yield return StartCoroutine(InitLevel(currentLevel));
            yield return StartCoroutine(PlayLevel());

            if (timer <= 0 && animalsList.Count > 0)
            {
                InstructionText.text = "GAME OVER...";
                yield return _waitForSeconds5;
                PauseMenu.GoToLobby();
                yield break;
            }

            currentLevel++;
        }

        InstructionText.text = "CONGRATULATIONS! YOU ARE A MASTER OF EMOTIONS!";

        yield return _waitForSeconds5;
        PauseMenu.GoToLobby();
    }

    private IEnumerator InitLevel(int level)
    {
        isLevelActive = false;
        string newAnimalInfo = string.Empty;

        switch (level)
        {
            case 1: newAnimalInfo = "Cat -> Happiness"; break;
            case 2: newAnimalInfo = "Deer -> Surprise"; break;
            case 3: newAnimalInfo = "Tiger -> Anger"; break;
            case 4: newAnimalInfo = "Penguin -> Sadness"; break;
            case 5: newAnimalInfo = "Spider -> Fear"; break;
        }

        InstructionText.text = $"LEVEL {level}\n\n{newAnimalInfo}";
        yield return _waitForSeconds5;

        timer = GetLevelTime(level);
        animalsList = GenerateAnimals(level);
        isLevelActive = true;
    }

    private IEnumerator PlayLevel()
    {
        InstructionText.text = string.Empty;
        SpawnAnimal();

        while (isLevelActive && timer > 0)
        {
            timer -= Time.deltaTime;
            string text = $"Time: 0:{Mathf.CeilToInt(timer)}";
            TimerText.SetText(text);

            if (animalsList.Count == 0 && currentAnimalGO == null)
            {
                isLevelActive = false;
                InstructionText.text = "LEVEL COMPLETED !";
                yield return _waitForSeconds2;
            }
            yield return null;
        }
    }

    private static List<AnimalType> GenerateAnimals(int level)
    {
        List<AnimalType> pool = new() { AnimalType.Cat };
        if (level >= 2) pool.Add(AnimalType.Deer);
        if (level >= 3) pool.Add(AnimalType.Tiger);
        if (level >= 4) pool.Add(AnimalType.Penguin);
        if (level >= 5) pool.Add(AnimalType.Spider);

        List<AnimalType> result = new();
        int totalTarget = GetAnimalNumber(level);
        const int minPerAnimal = 1;

        AnimalType firstAnimal = (AnimalType)(level - 1);
        Debug.Log($"First animal for level {level} is {firstAnimal}");
        result.Add(firstAnimal);

        foreach (AnimalType type in pool)
        {
            for (int i = 0; i < minPerAnimal; i++)
            {
                if (result.Count < totalTarget)
                {
                    result.Add(type);
                }
            }
        }

        while (result.Count < totalTarget)
        {
            result.Add(pool[Random.Range(0, pool.Count)]);
        }

        Shuffle(result);
        return result;
    }

    // Fisher-Yates shuffle algorithm to randomize the order of animals in the list
    private static void Shuffle<T>(List<T> list)
    {
        for (int i = 1; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    private void SpawnAnimal()
    {
        if (animalsList.Count == 0)
        {
            Debug.Log("LEVEL COMPLETE");
            isLevelActive = true;
            return;
        }

        AnimalType animal = animalsList[0];

        Quaternion rotation180 = Quaternion.Euler(0f, 180f, 0f);

        currentAnimalGO = Instantiate(AnimalPrefabs[(int)animal], SpawnPoint.position, rotation180);
        Debug.Log($"Emotion: {animalEmotions[animal]} for {animal}");

        CheckEmotion(animal);
    }

    private void UpdateScore()
    {
        score++;
        if (ScoreText != null)
        {
            string tmp = $"Score: {score}";
            ScoreText.SetText(tmp);
        }
    }

    private void CheckEmotion(AnimalType animal)
    {
        currentTargetAnimal = animal;
        isTrackingEmotion = true;
        currentProgress = 0f;
        if (EmotionProgressBar != null) EmotionProgressBar.value = 0f;
    }

    private IEnumerator WaitAndSpawn()
    {
        yield return _waitForSeconds1_5;
        SpawnAnimal();
    }

    private static float GetLevelTime(int level)
    {
        if (level == 1) return 35f;
        if (level == 2) return 40f;
        if (level == 3) return 50f;
        if (level == 4) return 60f;
        return 75f;
    }

    private static int GetAnimalNumber(int level)
    {
        return level switch
        {
            1 => 1,
            2 => 2,
            3 => 3,
            4 => 4,
            5 => 5,
            _ => 1,
        };
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
        if (!isLevelActive || !isTrackingEmotion || currentAnimalGO == null) return;

        EmotionType? dominantEmotion = UFeelAPI.Emotion.Dominant;
        EmotionType targetEmotion = animalEmotions[currentTargetAnimal];

        if (dominantEmotion == targetEmotion)
        {
            currentProgress += Time.deltaTime / MaxProgressDuration;
            if (barFillImage != null) barFillImage.color = Color.green;
        }
        else
        {
            currentProgress -= Time.deltaTime / MaxProgressDuration / 3;
            if (barFillImage != null) barFillImage.color = Color.red;
        }

        currentProgress = Mathf.Clamp01(currentProgress);

        if (EmotionProgressBar != null) EmotionProgressBar.value = currentProgress;

        if (currentProgress >= 1f)
        {
            isTrackingEmotion = false;
            OnAnimalSuccess();
        }
    }

    private void OnAnimalSuccess()
    {
        Debug.Log("CORRECT EMOTION MAINTAINED FOR 5S!");
        UpdateScore();

        Destroy(currentAnimalGO);
        animalsList.RemoveAt(0);

        StartCoroutine(WaitAndSpawn());
    }
}
