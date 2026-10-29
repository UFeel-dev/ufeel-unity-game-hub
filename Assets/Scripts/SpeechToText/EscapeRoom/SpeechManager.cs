using System.Collections;
using TMPro;
using UFeel;
using UnityEngine;

public class SpeechManager : MonoBehaviour
{
    private enum EscapeStep
    {
        Light,
        Radio,
        Window,
        BlueLight,
        TV,
        Door,
        Finished
    }

    [Header("Scene References")]
    [SerializeField] private VoiceDoorController doorController;
    [SerializeField] private VoiceWindowController windowController;
    [SerializeField] private AudioSource radioAudio;
    [SerializeField] private AudioSource windAudio;
    [SerializeField] private AudioSource tvAudio;
    [SerializeField] private Light roomLight;
    [SerializeField] private Light dummyLight;
    [SerializeField] private Light blueLight;

    [Header("TV System")]
    [SerializeField] private Renderer tvRenderer;
    [SerializeField] private Material tvBlueMaterial;

    [Header("Scene Hints")]
    [SerializeField] private GameObject radioHintText;
    [SerializeField] private GameObject windowHintText;
    [SerializeField] private GameObject windowHintTextLeft;
    [SerializeField] private GameObject windowHintTextRight;
    [SerializeField] private GameObject tvHintText;
    [SerializeField] private GameObject bookHintText;

    [Header("End Game")]
    [SerializeField] private CanvasGroup endCanvasGroup;
    [SerializeField] private TextMeshProUGUI endText;
    [SerializeField] private float fadeDuration = 5f;

    [Header("Hints System")]
    [SerializeField] private CanvasGroup hintCanvasGroup;
    [SerializeField] private TextMeshProUGUI hintText;

    [Header("Speech Recognition")]
    // [SerializeField] private float recognitionWindowSeconds = 4f;
    // [SerializeField] [Range(0.5f, 1f)] private float minimumCommandScore = 0.75f;

    private readonly SpeechCommandRecognizer commandRecognizer =
        new(4f, 0.75f);

    private EscapeStep currentStep;

    private Coroutine hintCoroutine;
    private Coroutine activeShowHintCoroutine;
    private Coroutine radioLoopCoroutine;

    private string lastProcessedSpeech = string.Empty;

    private static readonly WaitForSeconds Wait60 = new(60f);
    private static readonly WaitForSeconds Wait5 = new(5f);
    private static readonly WaitForSeconds Wait3 = new(3f);
    private static readonly WaitForSeconds Wait2 = new(2f);

    // private static readonly string[] VerbsLight = new[] { "allume", "allumer", "active", "activer" };
    // private static readonly string[] WordsLight = new[] { "lumière", "lumiere", "lampe", "éclairage", "eclairage" };

    private void Start()
    {
        UFeelDebugHUD.Clear();

        StartSpeechDetection();

        radioLoopCoroutine = StartCoroutine(PlayRadioWithDelay());

        SetStep(EscapeStep.Light);
    }

    private void Update()
    {
        HandleInput();

        string? currentSpeech = UFeelAPI.SpeechToText.Current;

        if (string.IsNullOrWhiteSpace(currentSpeech))
            return;

        if (currentSpeech == lastProcessedSpeech)
            return;

        SpeechRecognitionResult result =
            commandRecognizer.ProcessSpeech(currentSpeech);

        if (!result.IsValid)
            return;

        lastProcessedSpeech = currentSpeech;

        ExecuteCurrentStep();
    }

    private void HandleInput()
    {
        if (Input.GetKeyUp(KeyCode.V))
            StartSpeechDetection();

        if (Input.GetKeyDown(KeyCode.H))
            DisplayHint();
    }

    private static void StartSpeechDetection()
    {
        UFeelAPI.SpeechToText.StartDetection();
    }

    private void SetStep(EscapeStep step)
    {
        currentStep = step;

        SpeechCommand? command = CreateCommand(step);

        commandRecognizer.SetCommand(command);

        lastProcessedSpeech = string.Empty;

        StartHintTimer(GetHint(step));
    }

    private static SpeechCommand? CreateCommand(EscapeStep step)
    {
        return step switch
        {
            EscapeStep.Light => CreateLightCommand(),
            EscapeStep.Radio => CreateRadioCommand(),
            EscapeStep.Window => CreateWindowCommand(),
            EscapeStep.BlueLight => CreateBlueLightCommand(),
            EscapeStep.TV => CreateTvCommand(),
            EscapeStep.Door => CreateDoorCommand(),
            _ => null,
        };
    }

    private static SpeechCommand CreateLightCommand()
    {
        return new SpeechCommand(
            "LightOn",
            new[]
            {
                // new SpeechConcept(
                //     "TurnOn",
                //     new[] { "allume", "allumer", "active", "activer" },
                //     0.72f,
                //     1f),

                new SpeechConcept(
                    "Light",
                    new[] { "lumière", "lumiere", "lampe", "éclairage", "eclairage" },
                    0.72f,
                    1f)
            });
    }

    private static SpeechCommand CreateRadioCommand()
    {
        return new SpeechCommand(
            "RadioOff",
            new[]
            {
                // new SpeechConcept(
                //     "TurnOff",
                //     new[] { "éteins", "eteins", "éteindre", "eteindre", "coupe", "couper", "arrête", "arrete" },
                //     0.68f,
                //     1f),

                new SpeechConcept(
                    "Radio",
                    new[] { "radio" },
                    0.78f,
                    1f)
            });
    }

    private static SpeechCommand CreateWindowCommand()
    {
        return new SpeechCommand(
            "WindowClose",
            new[]
            {
                // new SpeechConcept(
                //     "Close",
                //     new[] { "ferme", "fermer" },
                //     0.72f,
                //     1f),

                new SpeechConcept(
                    "Window",
                    new[] { "fenêtre", "fenetre" },
                    0.72f,
                    1f)
            });
    }

    private static SpeechCommand CreateBlueLightCommand()
    {
        return new SpeechCommand(
            "PurpleLight",
            new[]
            {
                new SpeechConcept(
                    "Light",
                    new[] { "lumière", "lumiere", "lampe", "éclairage", "eclairage" },
                    0.72f,
                    1f),

                // new SpeechConcept(
                //     "Purple",
                //     new[] { "violette", "violet" },
                //     0.72f,
                //     1f)
            });
    }

    private static SpeechCommand CreateTvCommand()
    {
        return new SpeechCommand(
            "TvOn",
            new[]
            {
                new SpeechConcept(
                    "TurnOn",
                    new[] { "allume", "allumer", "active", "activer" },
                    0.72f,
                    1f),

                // new SpeechConcept(
                //     "Screen",
                //     new[] { "écran", "ecran", "télé", "tele", "télévision", "television" },
                //     0.72f,
                //     1f)
            });
    }

    private static SpeechCommand CreateDoorCommand()
    {
        return new SpeechCommand(
            "DoorOpen",
            new[]
            {
                new SpeechConcept(
                    "Open",
                    new[] { "ouvre", "ouvrir" },
                    0.72f,
                    1f),

                // new SpeechConcept(
                //     "Door",
                //     new[] { "porte" },
                //     0.78f,
                //     1f)
            });
    }

    private void ExecuteCurrentStep()
    {
        switch (currentStep)
        {
            case EscapeStep.Light:
                ExecuteLightAction();
                break;

            case EscapeStep.Radio:
                ExecuteRadioAction();
                break;

            case EscapeStep.Window:
                ExecuteWindowAction();
                break;

            case EscapeStep.BlueLight:
                ExecuteBlueLightAction();
                break;

            case EscapeStep.TV:
                ExecuteTvAction();
                break;

            case EscapeStep.Door:
                ExecuteDoorAction();
                break;
        }
    }

    private void ExecuteLightAction()
    {
        KillHint();

        roomLight.enabled = true;
        dummyLight.enabled = true;

        radioHintText.SetActive(true);
        windowHintTextLeft.SetActive(true);
        windowHintTextRight.SetActive(true);

        SetStep(EscapeStep.Radio);
    }

    private void ExecuteRadioAction()
    {
        KillHint();

        if (radioLoopCoroutine != null)
            StopCoroutine(radioLoopCoroutine);

        if (radioAudio != null)
            radioAudio.Stop();

        if (radioHintText != null)
            radioHintText.SetActive(false);

        SetStep(EscapeStep.Window);
    }

    private void ExecuteWindowAction()
    {
        KillHint();

        if (windowController != null)
        {
            windowController.CloseWindow();
            StartCoroutine(SlideWindowTexts(1.5f));
        }

        if (windowHintText != null)
        {
            StartCoroutine(FadeInText(windowHintText, 6f));

            if (windAudio != null)
                windAudio.Stop();
        }

        SetStep(EscapeStep.BlueLight);
    }

    private void ExecuteBlueLightAction()
    {
        KillHint();

        if (blueLight != null &&
            roomLight != null &&
            dummyLight != null &&
            bookHintText != null)
        {
            roomLight.enabled = false;
            dummyLight.enabled = false;
            blueLight.enabled = true;

            if (windowHintText != null)
                windowHintText.SetActive(false);

            StartCoroutine(FadeInText(bookHintText, 3f));
        }

        SetStep(EscapeStep.TV);
    }

    private void ExecuteTvAction()
    {
        KillHint();

        if (blueLight != null)
            blueLight.enabled = false;

        if (bookHintText != null)
            bookHintText.SetActive(false);

        if (tvRenderer != null && tvBlueMaterial != null)
            StartCoroutine(TvOn(1.5f));

        if (tvHintText != null)
            StartCoroutine(FadeInText(tvHintText, 3f));

        SetStep(EscapeStep.Door);
    }

    private void ExecuteDoorAction()
    {
        KillHint();

        if (doorController != null)
            doorController.OpenDoor();

        if (roomLight != null)
            roomLight.enabled = true;

        SetStep(EscapeStep.Finished);

        StartCoroutine(ShowEndScreen());
    }

    private static string GetHint(EscapeStep step)
    {
        return step switch
        {
            EscapeStep.Light => "Dites :\n\"allume la lumière\"",
            EscapeStep.Radio => "Dites :\n\"éteins la radio\"",
            EscapeStep.Window => "Dites :\n\"ferme la fenêtre\"",
            EscapeStep.BlueLight => "Dites :\n\"lumière violette\"",
            EscapeStep.TV => "Dites :\n\"allume l'écran\"",
            EscapeStep.Door => "Dites :\n\"ouvre la porte\"",
            _ => string.Empty,
        };
    }

    private void DisplayHint()
    {
        string hint = GetHint(currentStep);

        if (string.IsNullOrEmpty(hint))
            return;

        if (activeShowHintCoroutine != null)
            StopCoroutine(activeShowHintCoroutine);

        activeShowHintCoroutine = StartCoroutine(ShowHint(hint));
    }

    private void StartHintTimer(string command)
    {
        if (hintCoroutine != null)
            StopCoroutine(hintCoroutine);

        hintCoroutine = StartCoroutine(HintTimerCoroutine(command));
    }

    private void KillHint()
    {
        if (hintCoroutine != null)
        {
            StopCoroutine(hintCoroutine);
            hintCoroutine = null;
        }

        if (activeShowHintCoroutine != null)
        {
            StopCoroutine(activeShowHintCoroutine);
            activeShowHintCoroutine = null;
        }

        if (hintCanvasGroup != null)
        {
            hintCanvasGroup.alpha = 0f;
            hintCanvasGroup.gameObject.SetActive(false);
        }

        if (hintText != null)
            hintText.text = string.Empty;
    }

    private IEnumerator ShowHint(string text)
    {
        if (hintText == null || hintCanvasGroup == null)
            yield break;

        hintText.text = text;
        hintCanvasGroup.alpha = 1f;
        hintCanvasGroup.gameObject.SetActive(true);
    }

    private IEnumerator HintTimerCoroutine(string command)
    {
        yield return Wait60;

        activeShowHintCoroutine = StartCoroutine(ShowHint(command));
    }

    private static IEnumerator FadeInText(GameObject textObject, float duration)
    {
        if (textObject == null)
            yield break;

        yield return Wait3;

        if (!textObject.TryGetComponent(out TextMeshPro tmp))
            yield break;

        for (float time = 0f; time < duration; time += Time.deltaTime)
        {
            tmp.alpha = time / duration;
            yield return null;
        }

        tmp.alpha = 1f;
    }

    private IEnumerator SlideWindowTexts(float duration)
    {
        if (windowHintTextLeft == null || windowHintTextRight == null)
            yield break;

        Vector3 leftStart = windowHintTextLeft.transform.position;
        Vector3 rightStart = windowHintTextRight.transform.position;

        Vector3 leftTarget = new(22.16f, leftStart.y, leftStart.z);
        Vector3 rightTarget = new(27.14f, rightStart.y, rightStart.z);

        for (float time = 0f; time < duration; time += Time.deltaTime)
        {
            float progress = time / duration;

            windowHintTextLeft.transform.position =
                Vector3.Lerp(leftStart, leftTarget, progress);

            windowHintTextRight.transform.position =
                Vector3.Lerp(rightStart, rightTarget, progress);

            yield return null;
        }

        windowHintTextLeft.SetActive(false);
        windowHintTextRight.SetActive(false);
    }

    private IEnumerator TvOn(float duration)
    {
        if (tvRenderer == null)
            yield break;

        Material material = tvRenderer.material;

        material.EnableKeyword("_EMISSION");

        for (float time = 0f; time < duration; time += Time.deltaTime)
        {
            Color color = Color.Lerp(
                Color.black,
                Color.white,
                time / duration);

            material.SetColor("_BaseColor", color);
            material.SetColor("_EmissionColor", color * 3f);

            yield return null;
        }

        if (tvAudio != null)
            tvAudio.Play();
    }

    private IEnumerator ShowEndScreen()
    {
        yield return Wait2;

        if (endCanvasGroup != null)
        {
            for (float time = 0f; time < fadeDuration; time += Time.deltaTime)
            {
                endCanvasGroup.alpha = time / fadeDuration;
                yield return null;
            }

            endCanvasGroup.alpha = 1f;
        }

        yield return Wait5;

        PauseMenu.GoToLobby();
    }

    private IEnumerator PlayRadioWithDelay()
    {
        while (currentStep is EscapeStep.Light or EscapeStep.Radio)
        {
            if (radioAudio != null && radioAudio.clip != null)
            {
                radioAudio.Play();
                yield return new WaitForSeconds(radioAudio.clip.length);
            }

            yield return Wait5;
        }
    }
}
