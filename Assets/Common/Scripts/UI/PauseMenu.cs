using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameHub.Common
{
    public class PauseMenu : MonoBehaviour
    {
        private const float ScrimOpacity = 0.82f;
        private const int MaxRows = 5;

        private static readonly KeyHint[] Hints =
        {
            new("Select", 0, KeyHint.Up, KeyHint.Down),
            new("Confirm", 0, "Enter"),
            new("Resume", 0, "M"),
        };

        private readonly PointerInput mouse = new();
        private readonly List<MenuEntry> entries = new();
        private readonly List<PauseAction> actions = new();

        private Scrim scrim;
        private ListScreen screen;
        private SettingsPanel settings;
        private KeyBar keys;
        private bool paused;
        private CursorLockMode previousLockState;
        private bool previousCursorVisible;

        private enum PauseAction
        {
            Resume,
            Settings,
            Microphone,
            Lobby,
            Quit,
        }

        private void Awake()
        {
            UiKit kit = UiKit.Shared;
            RectTransform canvas = UiCanvas.Create("Pause", transform, UiCanvas.PauseOrder);
            scrim = new Scrim(kit, canvas, ScrimOpacity);
            screen = new ListScreen(kit, canvas, "Pause", MaxRows);
            keys = new KeyBar(kit, canvas);
            keys.Set(Hints);
            keys.SetVisible(false);
            settings = new SettingsPanel(kit, canvas);
        }

        private void OnDestroy()
        {
            if (!paused)
                return;

            settings.Hide();
            Time.timeScale = 1f;
            UiFocus.Release(this);
            paused = false;
        }

        private void Update()
        {
            float deltaTime = Time.unscaledDeltaTime;
            mouse.Tick();
            scrim.Tick(deltaTime);
            screen.Tick(deltaTime);
            settings.Tick(deltaTime, mouse);

            if (!paused)
            {
                if (UiFocus.IsFree && LayoutInput.GetKeyDown(KeyCode.M))
                    Pause();
                return;
            }

            bool onTop = UiFocus.IsTop(this);
            screen.SetVisible(onTop);
            keys.SetVisible(onTop);
            if (!UiFocus.Has(this))
                return;

            if (LayoutInput.GetKeyDown(KeyCode.M))
            {
                Resume();
                return;
            }

            switch (screen.List.HandleInput(deltaTime, mouse))
            {
                case MenuAction.Back:
                    Resume();
                    break;
                case MenuAction.Confirmed:
                    Run(actions[screen.List.Selected]);
                    break;
            }
        }

        private void Resume()
        {
            scrim.Show(false, Fader.DefaultSeconds);
            screen.Hide();
            keys.SetVisible(false);
            Time.timeScale = 1f;
            Cursor.lockState = previousLockState;
            Cursor.visible = previousCursorVisible;
            UiFocus.Release(this);
            paused = false;
        }

        private void Pause()
        {
            previousLockState = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
            scrim.Show(true, Fader.DefaultSeconds);
            BuildEntries();
            screen.Show("Paused", entries, 0);
            keys.SetVisible(true);
            Time.timeScale = 0f;
            PointerInput.ShowCursor(true);
            UiFocus.Capture(this);
            paused = true;
        }

        private void BuildEntries()
        {
            entries.Clear();
            actions.Clear();
            Add("Resume", PauseAction.Resume);
            Add("Settings", PauseAction.Settings);
            Add("Microphone", PauseAction.Microphone);
            if (SceneManager.GetActiveScene().name != SceneNames.Hub)
                Add("Lobby", PauseAction.Lobby);
            Add("Quit", PauseAction.Quit);
        }

        private void Add(string label, PauseAction action)
        {
            entries.Add(new MenuEntry(label));
            actions.Add(action);
        }

        private void Run(PauseAction action)
        {
            switch (action)
            {
                case PauseAction.Resume:
                    Resume();
                    break;
                case PauseAction.Settings:
                    settings.Show();
                    break;
                case PauseAction.Microphone:
                    UFeelScreens.ShowMicrophone();
                    break;
                case PauseAction.Lobby:
                    Resume();
                    GameFlow.ReturnToHub();
                    break;
                default:
                    GameFlow.Quit();
                    break;
            }
        }
    }
}
