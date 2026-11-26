using UnityEngine;
using UnityEngine.UI;

namespace GameHub.RhythmGame
{
    public sealed class StageView
    {
        public const int CalibrationTaps = 16;

        private const int SortingOrder = 100;
        private const float ScrimAlpha = 0.82f;
        private static readonly Vector2 ReferenceResolution = new(1920f, 1080f);

        private readonly BackdropView backdrop;
        private readonly Image scrim;

        public StageView(Transform owner, UiKit kit)
        {
            GameObject root = new("RhythmStage", typeof(RectTransform)) { layer = 5 };
            root.transform.SetParent(owner, false);

            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortingOrder;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent;

            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;

            Transform parent = root.transform;
            backdrop = new BackdropView(parent, kit.Sprites);
            Playfield = new PlayfieldView(kit, parent);
            Hud = new HudView(kit, parent);
            scrim = kit.NewSolid("Scrim", parent, Theme.Fade(Theme.Backdrop, ScrimAlpha));
            UiLayout.Stretch(scrim.rectTransform);
            scrim.enabled = false;
            SongSelect = new SongSelectView(kit, parent);
            Results = new ResultsView(kit, parent);
            Options = new ListScreen(kit, parent, "Options", OptionsController.RowCount);
            Keys = new KeyBindingView(kit, parent);
            Calibration = new CalibrationView(kit, parent, CalibrationTaps);
            Pause = new ListScreen(kit, parent, "Pause", 3);
            HudLayout = new HudLayoutView(kit, parent);
            KeyBar = new KeyBar(kit, parent);
            Overlay = new OverlayView(kit, parent);
        }

        public PlayfieldView Playfield { get; }

        public HudView Hud { get; }

        public SongSelectView SongSelect { get; }

        public ResultsView Results { get; }

        public ListScreen Options { get; }

        public KeyBindingView Keys { get; }

        public CalibrationView Calibration { get; }

        public ListScreen Pause { get; }

        public HudLayoutView HudLayout { get; }

        public KeyBar KeyBar { get; }

        public OverlayView Overlay { get; }

        public void ShowGameplay(bool visible)
        {
            Playfield.SetVisible(visible);
            Hud.SetVisible(visible);
            backdrop.SetPlaying(visible);
            if (!visible)
                Playfield.Clear();
        }

        public void ShowScrim(bool visible)
        {
            scrim.enabled = visible;
        }

        public void HideScreens()
        {
            SongSelect.SetVisible(false);
            Results.SetVisible(false);
            Options.Hide();
            Keys.SetVisible(false);
            Calibration.SetVisible(false);
            Pause.Hide();
            HudLayout.SetVisible(false);
            ShowScrim(false);
        }

        public void Tick(float deltaTime, Color tint, float pulse, float flashIntensity)
        {
            backdrop.Tick(deltaTime, tint, pulse, flashIntensity);
            Options.Tick(deltaTime);
            Pause.Tick(deltaTime);
            Keys.Tick(deltaTime);
            Overlay.Tick(deltaTime);
        }
    }
}
