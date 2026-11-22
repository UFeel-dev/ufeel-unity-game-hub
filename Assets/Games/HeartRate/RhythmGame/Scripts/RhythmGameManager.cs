using System.Collections;
using GameHub.Common;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.RhythmGame
{
    public class RhythmGameManager : MonoBehaviour
    {
        private const int InitialMultiplierThreshold = 4;
        private const int HealthGainPerMultiplier = 1;
        private const int HealthLossPerMiss = 5;
        private const float MusicVolume = 0.5f;
        private const float EcgBeepVolume = 0.25f;

        private static readonly WaitForSeconds BpmRefreshDelay = new(1f);
        private static readonly WaitForSeconds ReturnToHubDelay = new(5f);

        [Header("Gameplay")]
        [SerializeField] private BeatScroller beatScroller;
        [SerializeField] private int scorePerNote = 100;

        [Header("UI")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Text multiplierText;
        [SerializeField] private Slider healthSlider;

        [Header("Audio")]
        [SerializeField] private AudioSource music;
        [SerializeField] private AudioSource ecgBeep;

        private Coroutine ecgBeepRoutine;
        private bool hasStarted;
        private bool isGameOver;
        private int score;
        private int multiplier = 1;
        private int hitsTowardsMultiplier;
        private int multiplierThreshold = InitialMultiplierThreshold;

        public static RhythmGameManager? Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            music.volume = MusicVolume;
            ecgBeep.volume = EcgBeepVolume;

            StartCoroutine(FollowHeartRate());
            ecgBeepRoutine = StartCoroutine(PlayEcgBeep());
        }

        private void Update()
        {
            if (isGameOver)
                return;

            if (!hasStarted && LayoutInput.AnyKeyDown())
            {
                hasStarted = true;
                beatScroller.IsScrolling = true;
                music.Play();
            }

            if (hasStarted && healthSlider.value <= healthSlider.minValue)
                GameOver();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void NoteHit()
        {
            if (isGameOver)
                return;

            hitsTowardsMultiplier++;
            if (hitsTowardsMultiplier >= multiplierThreshold)
            {
                hitsTowardsMultiplier = 0;
                multiplierThreshold *= 2;
                multiplier++;
            }

            score += scorePerNote * multiplier;
            scoreText.text = $"Score: {score}";
            multiplierText.text = $"Multiplier: x{multiplier}";
            ChangeHealth(HealthGainPerMultiplier * multiplier);
        }

        public void NoteMissed()
        {
            if (isGameOver)
                return;

            multiplier = 1;
            hitsTowardsMultiplier = 0;
            multiplierThreshold = InitialMultiplierThreshold;
            multiplierText.text = $"Multiplier: x{multiplier}";
            ChangeHealth(-HealthLossPerMiss);
        }

        private void ChangeHealth(float amount)
        {
            healthSlider.value = Mathf.Clamp(healthSlider.value + amount, healthSlider.minValue, healthSlider.maxValue);
        }

        private void GameOver()
        {
            isGameOver = true;
            beatScroller.IsScrolling = false;
            music.Stop();
            StopCoroutine(ecgBeepRoutine);

            scoreText.text = $"<color=red>Game Over</color>\nScore: {score}";
            StartCoroutine(ReturnToHub());
        }

        private static IEnumerator ReturnToHub()
        {
            yield return ReturnToHubDelay;
            GameFlow.ReturnToHub();
        }

        private IEnumerator FollowHeartRate()
        {
            while (true)
            {
                uint bpm = HeartRateTempo.TryGetBpm(out uint reading) ? reading : HeartRateTempo.MinBpm;
                beatScroller.Bpm = bpm;
                music.pitch = bpm / (float)HeartRateTempo.MinBpm;
                yield return BpmRefreshDelay;
            }
        }

        private IEnumerator PlayEcgBeep()
        {
            while (true)
            {
                ecgBeep.Play();
                yield return new WaitForSeconds(60f / beatScroller.Bpm);
            }
        }
    }
}

// TODO: fix the issue where the arrows aren't always rotated properly when changing lane
