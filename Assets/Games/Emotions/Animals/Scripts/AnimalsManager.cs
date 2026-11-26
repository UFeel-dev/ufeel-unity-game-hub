using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using GameHub.Common;
using UFeel;
using UnityEngine;

namespace GameHub.Animals
{
    public class AnimalsManager : MonoBehaviour
    {
        private const float EmotionHoldDuration = 2.5f;
        private const float EmotionDecayRatio = 1f / 3f;

        private static readonly AnimalDefinition[] Animals =
        {
            new(AnimalType.Cat, EmotionType.Happy, "Happiness"),
            new(AnimalType.Deer, EmotionType.Surprised, "Surprise"),
            new(AnimalType.Tiger, EmotionType.Angry, "Anger"),
            new(AnimalType.Penguin, EmotionType.Sad, "Sadness"),
            new(AnimalType.Spider, EmotionType.Fearful, "Fear"),
        };

        private static readonly float[] LevelDurations = { 35f, 40f, 50f, 60f, 75f };

        private static readonly WaitForSeconds MessageDelay = new(5f);
        private static readonly WaitForSeconds LevelCompletedDelay = new(2f);
        private static readonly WaitForSeconds NextAnimalDelay = new(1.5f);

        [Header("Scene References")]
        [SerializeField] private Transform spawnPoint;
        [Tooltip("One prefab per AnimalType, in the enum order.")]
        [SerializeField] private GameObject[] animalPrefabs;

        private readonly List<AnimalType> remainingAnimals = new();
        private AnimalsHud hud;
        private GameObject currentAnimal;
        private AnimalType currentAnimalType;
        private float timer;
        private float progress;
        private int score;
        private bool isLevelActive;
        private bool isTrackingEmotion;

        private void Awake()
        {
            hud = new AnimalsHud(UiKit.Shared, transform, Animals.Length);
        }

        private void Start()
        {
            if (animalPrefabs.Length != Animals.Length)
            {
                Debug.LogError($"{nameof(AnimalsManager)} needs {Animals.Length} animal prefabs, one per {nameof(AnimalType)}.", this);
                enabled = false;
                return;
            }

            StartCoroutine(GameLoop());
        }

        private void Update()
        {
            hud.Tick(Time.deltaTime);
            if (!isLevelActive || !isTrackingEmotion || currentAnimal == null)
                return;

            bool isExpectedEmotion = UFeelAPI.Emotion.Dominant == Animals[(int)currentAnimalType].Emotion;
            float step = Time.deltaTime / EmotionHoldDuration;
            progress = Mathf.Clamp01(isExpectedEmotion ? progress + step : progress - (step * EmotionDecayRatio));

            hud.SetProgress(progress, isExpectedEmotion);

            if (progress >= 1f)
                OnAnimalMatched();
        }

        private static List<AnimalType> ShuffledAnimalsForLevel(int level)
        {
            AnimalType newAnimal = Animals[level - 1].Type;
            List<AnimalType> animals = new();
            for (int i = 0; i < level - 1; i++)
                animals.Add(Animals[i].Type);

            for (int i = animals.Count - 1; i > 0; i--)
            {
                int swapIndex = Random.Range(0, i + 1);
                (animals[i], animals[swapIndex]) = (animals[swapIndex], animals[i]);
            }

            animals.Insert(0, newAnimal);
            return animals;
        }

        private IEnumerator GameLoop()
        {
            for (int level = 1; level <= Animals.Length; level++)
            {
                yield return StartLevel(level);
                yield return PlayLevel();

                if (remainingAnimals.Count > 0)
                {
                    yield return EndGame("Game over", "Back to the hub", Theme.Danger);
                    yield break;
                }
            }

            yield return EndGame("Congratulations", "You are a master of emotions!", Theme.Accent);
        }

        private IEnumerator StartLevel(int level)
        {
            AnimalDefinition newAnimal = Animals[level - 1];
            isLevelActive = false;
            hud.SetLevel(level);
            hud.ShowCard("Level " + level.ToString(CultureInfo.InvariantCulture), $"{newAnimal.Type}  ·  {newAnimal.EmotionLabel}", Theme.Accent);
            yield return MessageDelay;
            hud.HideCard();

            timer = LevelDurations[level - 1];
            hud.SetTime(timer);
            remainingAnimals.Clear();
            remainingAnimals.AddRange(ShuffledAnimalsForLevel(level));
            isLevelActive = true;
        }

        private IEnumerator PlayLevel()
        {
            SpawnNextAnimal();

            while (isLevelActive && timer > 0f)
            {
                timer -= Time.deltaTime;
                hud.SetTime(timer);

                if (remainingAnimals.Count == 0 && currentAnimal == null)
                {
                    isLevelActive = false;
                    hud.ShowCard("Level completed", string.Empty, Theme.Accent);
                    yield return LevelCompletedDelay;
                    hud.HideCard();
                }

                yield return null;
            }
        }

        private IEnumerator EndGame(string title, string subtitle, Color accent)
        {
            isLevelActive = false;
            hud.HideAnimal();
            hud.ShowCard(title, subtitle, accent);
            yield return MessageDelay;
            GameFlow.ReturnToHub();
        }

        private void SpawnNextAnimal()
        {
            if (remainingAnimals.Count == 0)
                return;

            currentAnimalType = remainingAnimals[0];
            currentAnimal = Instantiate(animalPrefabs[(int)currentAnimalType], spawnPoint.position, Quaternion.Euler(0f, 180f, 0f));
            isTrackingEmotion = true;
            progress = 0f;
            hud.ShowAnimal(currentAnimalType.ToString());
        }

        private void OnAnimalMatched()
        {
            isTrackingEmotion = false;
            score++;
            hud.SetScore(score);
            hud.HideAnimal();

            Destroy(currentAnimal);
            remainingAnimals.RemoveAt(0);
            StartCoroutine(SpawnNextAnimalAfterDelay());
        }

        private IEnumerator SpawnNextAnimalAfterDelay()
        {
            yield return NextAnimalDelay;
            SpawnNextAnimal();
        }

        private readonly struct AnimalDefinition
        {
            public AnimalDefinition(AnimalType type, EmotionType emotion, string emotionLabel)
            {
                Type = type;
                Emotion = emotion;
                EmotionLabel = emotionLabel;
            }

            public AnimalType Type { get; }

            public EmotionType Emotion { get; }

            public string EmotionLabel { get; }
        }
    }
}
