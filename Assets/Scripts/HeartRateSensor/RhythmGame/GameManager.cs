using System.Collections;
using UFeel;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds1 = new(1f);
    public static GameManager Instance;

    public bool StartPlaying;
    public BeatScroller Bs;

    public int CurrentScore;
    public int ScorePerNote = 100;

    public Text ScoreText;
    public Text MultiplierText;

    public int CurrentMultiplier = 1;
    public int MultiplierTracker = 0;
    public int MultiplierThreshold = 4;

    private const uint MinBpm = 60;
    private const uint MaxBpm = 180;
    private Coroutine actionCoroutine;
    private Coroutine actionCoroutine2;

    private bool simulated = false;
    private const int HpGainScore = 1;
    public Slider _slider;

    public AudioSource _music;
    public AudioSource _ECGBeep;

    private void Start()
    {
        Instance = this;
        _music.volume = 0.5f;
        _ECGBeep.volume = 0.25f;

        UFeelDebugHUD.Clear();

        InitializeDetection();

        actionCoroutine = StartCoroutine(UpdateBPM());
        actionCoroutine2 = StartCoroutine(PlayECGBeep());
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
        if (Input.GetKeyUp(KeyCode.N))
        {
            simulated = !simulated;
            UFeelAPI.HeartRate.ToggleHeartRateSimulation(simulated);
        }
        if (!StartPlaying && Input.anyKeyDown)
        {
            StartPlaying = true;
            Bs.Started = true;
            _music.Play();
        }
        if (_slider.value <= 0)
        {
            // DISPLAY GAME OVER SCREEN
        }
    }

    public void NoteHit()
    {
        MultiplierTracker++;

        if (MultiplierTracker >= MultiplierThreshold)
        {
            MultiplierTracker = 0;
            MultiplierThreshold *= 2;
            CurrentMultiplier++;
        }

        CurrentScore += ScorePerNote * CurrentMultiplier;
        ScoreText.text = "Score: " + CurrentScore;
        MultiplierText.text = "Multiplier: x" + CurrentMultiplier;
        _slider.value = Mathf.Clamp(_slider.value + (HpGainScore * CurrentMultiplier), _slider.minValue, _slider.maxValue);
    }

    public void NoteMissed()
    {
        CurrentMultiplier = 1;
        MultiplierTracker = 0;
        MultiplierThreshold = 4;
        MultiplierText.text = "Multiplier: x" + CurrentMultiplier;
        _slider.value = Mathf.Clamp(_slider.value - 5, _slider.minValue, _slider.maxValue);
    }

    private IEnumerator UpdateBPM()
    {
        while (true)
        {
            uint bpm = GetBPM();

            if (bpm == 0)
                yield break;

            Bs.Bpm = bpm;
            _music.pitch = bpm / (float)MinBpm;

            yield return _waitForSeconds1;
        }
    }

    private IEnumerator PlayECGBeep()
    {
        while (true)
        {
            _ECGBeep.Play();

            yield return new WaitForSeconds(1f / (Bs.Bpm / 60f));
        }
    }

    private static uint GetBPM()
    {
        uint currentBPM = UFeelAPI.HeartRate.Current;

        if (currentBPM == 0)
            return currentBPM;

        if (currentBPM < MinBpm)
            return MinBpm;

        if (currentBPM > MaxBpm)
            return MaxBpm;

        return currentBPM;
    }

    private void OnDestroy()
    {
        StopCoroutine(actionCoroutine);
        StopCoroutine(actionCoroutine2);
    }
}
