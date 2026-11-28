using System.Collections;
using GameHub.Common;
using TMPro;
using UFeel;
using UnityEngine;

namespace GameHub.EscapeRoom
{
    public class SpeechManager : MonoBehaviour
    {
        private const float RecognitionWindowSeconds = 4f;
        private const float MinimumCommandScore = 0.75f;
        private const float DefaultFuzzyThreshold = 0.72f;
        private const float TextFadeDelay = 3f;
        private const float HintOffset = 120f;
        private const float CommandVolume = 0.5f;
        private const float EndOpacity = 0.92f;
        private const string EndTitle = "Félicitations !";
        private const string EndSubtitle = "Vous avez réussi à vous échapper !";

        private static readonly string[] LightWords = { "lumière", "lumiere", "lampe", "éclairage", "eclairage" };

        private static readonly WaitForSeconds HintDelay = new(60f);
        private static readonly WaitForSeconds RadioPause = new(5f);
        private static readonly WaitForSeconds EndScreenDelay = new(2f);
        private static readonly WaitForSeconds ReturnToHubDelay = new(5f);

        [Header("Scene References")]
        [SerializeField] private VoiceDoorController doorController;
        [SerializeField] private VoiceWindowController windowController;
        [SerializeField] private AudioSource radioAudio;
        [SerializeField] private AudioSource windAudio;
        [SerializeField] private AudioSource tvAudio;
        [SerializeField] private Light roomLight;
        [SerializeField] private Light dummyLight;
        [SerializeField] private Light blueLight;

        [Header("TV")]
        [SerializeField] private Renderer tvRenderer;

        [Header("Scene Hints")]
        [SerializeField] private GameObject radioHintText;
        [SerializeField] private GameObject windowHintText;
        [SerializeField] private GameObject windowHintTextLeft;
        [SerializeField] private GameObject windowHintTextRight;
        [SerializeField] private float windowHintTextLeftTargetX = 22.16f;
        [SerializeField] private float windowHintTextRightTargetX = 27.14f;
        [SerializeField] private GameObject tvHintText;
        [SerializeField] private GameObject bookHintText;

        [Header("End Game")]
        [SerializeField] private float fadeDuration = 5f;

        private readonly SpeechCommandRecognizer commandRecognizer = new(RecognitionWindowSeconds, MinimumCommandScore);

        private EscapeStep currentStep;
        private Coroutine? hintTimerRoutine;
        private Coroutine radioLoopRoutine;
        private string lastProcessedSpeech = string.Empty;
        private Toast hint;
        private TitleCard endCard;

        private enum EscapeStep
        {
            Light,
            Radio,
            Window,
            BlueLight,
            Tv,
            Door,
            Finished,
        }

        private void Awake()
        {
            UiKit kit = UiKit.Shared;
            RectTransform canvas = UiCanvas.Create("EscapeRoomHud", transform, UiCanvas.GameOrder);
            hint = new Toast(kit, canvas, new Vector2(0.5f, 0f), HintOffset, Theme.Heading);
            endCard = new TitleCard(kit, canvas, EndOpacity);

            foreach (GameObject sceneHint in new[] { radioHintText, windowHintText, windowHintTextLeft, windowHintTextRight, tvHintText, bookHintText })
            {
                if (sceneHint != null && sceneHint.TryGetComponent(out TMP_Text text))
                    WorldText.Style(text, false, text.color);
            }
        }

        private void Start()
        {
            UFeelAPI.SpeechToText.StartDetection();
            radioLoopRoutine = StartCoroutine(LoopRadio());
            SetStep(EscapeStep.Light);
        }

        private void Update()
        {
            hint.Tick(Time.deltaTime);
            endCard.Tick(Time.deltaTime);

            if (KeyBinds.Up(KeyAction.RestartListening))
                UFeelAPI.SpeechToText.StartDetection();

            if (KeyBinds.Down(KeyAction.Hint))
                ShowHint(HintFor(currentStep));

            string? speech = UFeelAPI.SpeechToText.Current;
            if (string.IsNullOrWhiteSpace(speech) || speech == lastProcessedSpeech)
                return;

            if (!commandRecognizer.ProcessSpeech(speech).IsValid)
                return;

            lastProcessedSpeech = speech;
            GameSounds.Play(SoundCue.MenuConfirm, CommandVolume);
            ExecuteCurrentStep();
        }

        private static SpeechCommand? CommandFor(EscapeStep step)
        {
            return step switch
            {
                EscapeStep.Light => SingleWordCommand("LightOn", "Light", DefaultFuzzyThreshold, LightWords),
                EscapeStep.Radio => SingleWordCommand("RadioOff", "Radio", 0.78f, "radio"),
                EscapeStep.Window => SingleWordCommand("WindowClose", "Window", DefaultFuzzyThreshold, "fenêtre", "fenetre"),
                EscapeStep.BlueLight => SingleWordCommand("PurpleLight", "Light", DefaultFuzzyThreshold, LightWords),
                EscapeStep.Tv => SingleWordCommand("TvOn", "TurnOn", DefaultFuzzyThreshold, "allume", "allumer", "active", "activer"),
                EscapeStep.Door => SingleWordCommand("DoorOpen", "Open", DefaultFuzzyThreshold, "ouvre", "ouvrir"),
                _ => null,
            };
        }

        private static SpeechCommand SingleWordCommand(string id, string conceptName, float fuzzyThreshold, params string[] words)
        {
            return new SpeechCommand(id, new[] { new SpeechConcept(conceptName, words, fuzzyThreshold, 1f) });
        }

        private static string HintFor(EscapeStep step)
        {
            return step switch
            {
                EscapeStep.Light => "Dites :\n\"allume la lumière\"",
                EscapeStep.Radio => "Dites :\n\"éteins la radio\"",
                EscapeStep.Window => "Dites :\n\"ferme la fenêtre\"",
                EscapeStep.BlueLight => "Dites :\n\"lumière violette\"",
                EscapeStep.Tv => "Dites :\n\"allume l'écran\"",
                EscapeStep.Door => "Dites :\n\"ouvre la porte\"",
                _ => string.Empty,
            };
        }

        private static IEnumerator FadeInText(GameObject textObject, float duration)
        {
            if (textObject == null || !textObject.TryGetComponent(out TextMeshPro text))
                yield break;

            yield return new WaitForSeconds(TextFadeDelay);

            for (float time = 0f; time < duration; time += Time.deltaTime)
            {
                text.alpha = time / duration;
                yield return null;
            }

            text.alpha = 1f;
        }

        private void SetStep(EscapeStep step)
        {
            currentStep = step;
            SpeechCommand? command = CommandFor(step);
            if (command == null)
                return;
            commandRecognizer.SetCommand(command);
            lastProcessedSpeech = string.Empty;

            if (hintTimerRoutine != null)
                StopCoroutine(hintTimerRoutine);

            hintTimerRoutine = StartCoroutine(ShowHintAfterDelay(HintFor(step)));
        }

        private void ExecuteCurrentStep()
        {
            HideHint();

            switch (currentStep)
            {
                case EscapeStep.Light:
                    TurnOnLight();
                    break;
                case EscapeStep.Radio:
                    TurnOffRadio();
                    break;
                case EscapeStep.Window:
                    CloseWindow();
                    break;
                case EscapeStep.BlueLight:
                    TurnOnBlueLight();
                    break;
                case EscapeStep.Tv:
                    TurnOnTv();
                    break;
                case EscapeStep.Door:
                    OpenDoor();
                    break;
            }
        }

        private void TurnOnLight()
        {
            roomLight.enabled = true;
            dummyLight.enabled = true;
            radioHintText.SetActive(true);
            windowHintTextLeft.SetActive(true);
            windowHintTextRight.SetActive(true);
            SetStep(EscapeStep.Radio);
        }

        private void TurnOffRadio()
        {
            if (radioLoopRoutine != null)
                StopCoroutine(radioLoopRoutine);

            if (radioAudio != null)
                radioAudio.Stop();

            if (radioHintText != null)
                radioHintText.SetActive(false);

            SetStep(EscapeStep.Window);
        }

        private void CloseWindow()
        {
            if (windowController != null)
            {
                windowController.Close();
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

        private void TurnOnBlueLight()
        {
            if (blueLight != null && roomLight != null && dummyLight != null && bookHintText != null)
            {
                roomLight.enabled = false;
                dummyLight.enabled = false;
                blueLight.enabled = true;

                if (windowHintText != null)
                    windowHintText.SetActive(false);

                StartCoroutine(FadeInText(bookHintText, 3f));
            }

            SetStep(EscapeStep.Tv);
        }

        private void TurnOnTv()
        {
            if (blueLight != null)
                blueLight.enabled = false;

            if (bookHintText != null)
                bookHintText.SetActive(false);

            if (tvRenderer != null)
                StartCoroutine(FadeInTv(1.5f));

            if (tvHintText != null)
                StartCoroutine(FadeInText(tvHintText, 3f));

            SetStep(EscapeStep.Door);
        }

        private void OpenDoor()
        {
            if (doorController != null)
                doorController.Open();

            if (roomLight != null)
                roomLight.enabled = true;

            SetStep(EscapeStep.Finished);
            StartCoroutine(ShowEndScreen());
        }

        private void ShowHint(string text)
        {
            if (!string.IsNullOrEmpty(text))
                hint.Show(text, float.PositiveInfinity);
        }

        private void HideHint()
        {
            if (hintTimerRoutine != null)
            {
                StopCoroutine(hintTimerRoutine);
                hintTimerRoutine = null;
            }

            hint.Hide();
        }

        private IEnumerator ShowHintAfterDelay(string text)
        {
            yield return HintDelay;
            ShowHint(text);
        }

        private IEnumerator SlideWindowTexts(float duration)
        {
            if (windowHintTextLeft == null || windowHintTextRight == null)
                yield break;

            Vector3 leftStart = windowHintTextLeft.transform.position;
            Vector3 rightStart = windowHintTextRight.transform.position;
            Vector3 leftTarget = new(windowHintTextLeftTargetX, leftStart.y, leftStart.z);
            Vector3 rightTarget = new(windowHintTextRightTargetX, rightStart.y, rightStart.z);

            for (float time = 0f; time < duration; time += Time.deltaTime)
            {
                float progress = time / duration;
                windowHintTextLeft.transform.position = Vector3.Lerp(leftStart, leftTarget, progress);
                windowHintTextRight.transform.position = Vector3.Lerp(rightStart, rightTarget, progress);
                yield return null;
            }

            windowHintTextLeft.SetActive(false);
            windowHintTextRight.SetActive(false);
        }

        private IEnumerator FadeInTv(float duration)
        {
            Material material = tvRenderer.material;
            material.EnableKeyword("_EMISSION");

            for (float time = 0f; time < duration; time += Time.deltaTime)
            {
                Color color = Color.Lerp(Color.black, Color.white, time / duration);
                material.SetColor("_BaseColor", color);
                material.SetColor("_EmissionColor", color * 3f);
                yield return null;
            }

            if (tvAudio != null)
                tvAudio.Play();
        }

        private IEnumerator ShowEndScreen()
        {
            yield return EndScreenDelay;
            GameSounds.Play(SoundCue.Win);
            endCard.Show(EndTitle, EndSubtitle, Theme.Accent, fadeDuration);
            yield return new WaitForSeconds(fadeDuration);
            yield return ReturnToHubDelay;
            GameFlow.ReturnToHub();
        }

        private IEnumerator LoopRadio()
        {
            while (currentStep is EscapeStep.Light or EscapeStep.Radio)
            {
                if (radioAudio != null && radioAudio.clip != null)
                {
                    radioAudio.Play();
                    yield return new WaitForSeconds(radioAudio.clip.length);
                }

                yield return RadioPause;
            }
        }
    }
}
