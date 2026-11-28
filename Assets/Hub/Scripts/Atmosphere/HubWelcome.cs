using GameHub.Common;
using TMPro;
using UnityEngine;

namespace GameHub.Hub
{
    public sealed class HubWelcome
    {
        private const string SeenKey = "GameHub.Hub.WelcomeSeen";
        private const float ScrimOpacity = 0.85f;
        private const float Width = 1100f;
        private const int ContinueAction = 1;

        private static readonly KeyHint[] Hints = { new("Continue", ContinueAction, "Enter") };

        private static readonly (string Keys, string Action)[] Controls =
        {
            ("WASD / ZQSD", "Move"),
            ("Mouse", "Look around"),
            ("Tab", "Use the room menu"),
            ("E", "Enter the door in front of you"),
            ("M", "Pause and settings"),
            ("F8", "Calibrate eye tracking"),
            ("F1", "Camera and sensor data"),
        };

        private readonly RectTransform canvas;
        private readonly Scrim scrim;
        private readonly KeyBar keys;
        private readonly PointerInput mouse = new();

        private HubWelcome(UiKit kit, Transform owner)
        {
            canvas = UiCanvas.Create("HubWelcome", owner, UiCanvas.FullScreenGameOrder, false);
            scrim = new Scrim(kit, canvas, ScrimOpacity);
            scrim.Show(true);

            Vector2 center = new(0.5f, 0.5f);
            TextMeshProUGUI title = kit.NewText("Title", canvas, Theme.Display, true, TextAlignmentOptions.Center, Theme.Text);
            title.text = "Welcome to the Game Hub";
            UiLayout.Place(title.rectTransform, center, new Vector2(0f, 240f), new Vector2(Width, 96f));

            TextMeshProUGUI subtitle = kit.NewText("Subtitle", canvas, Theme.Heading, false, TextAlignmentOptions.Center, Theme.TextDim);
            subtitle.text = "Pick a room in the menu, walk to its door and press E.";
            UiLayout.Place(subtitle.rectTransform, center, new Vector2(0f, 160f), new Vector2(Width, 48f));

            for (int i = 0; i < Controls.Length; i++)
            {
                float y = 70f - (i * 48f);
                TextMeshProUGUI key = kit.NewText("Key", canvas, Theme.Body, true, TextAlignmentOptions.MidlineRight, Theme.Accent);
                key.text = Controls[i].Keys;
                UiLayout.Place(key.rectTransform, center, new Vector2(1f, 0.5f), new Vector2(-20f, y), new Vector2(400f, 44f));
                TextMeshProUGUI action = kit.NewText("Action", canvas, Theme.Body, false, TextAlignmentOptions.MidlineLeft, Theme.Text);
                action.text = Controls[i].Action;
                UiLayout.Place(action.rectTransform, center, new Vector2(0f, 0.5f), new Vector2(20f, y), new Vector2(500f, 44f));
            }

            keys = new KeyBar(kit, canvas);
            keys.Set(Hints);
            UiFocus.Capture(this);
        }

        public static HubWelcome? ShowOnFirstVisit(UiKit kit, Transform owner)
        {
            return PlayerPrefs.GetInt(SeenKey, 0) == 0 ? new HubWelcome(kit, owner) : null;
        }

        public bool Tick(float deltaTime)
        {
            mouse.Tick();
            scrim.Tick(deltaTime);
            if (!UiFocus.Has(this))
                return false;

            bool confirmed = MenuKeys.Confirm() || LayoutInput.GetKeyDown(KeyCode.Space) || keys.Clicked(mouse) == ContinueAction;
            if (!confirmed)
                return false;

            Close();
            PlayerPrefs.SetInt(SeenKey, 1);
            PlayerPrefs.Save();
            return true;
        }

        public void Close()
        {
            UiFocus.Release(this);
            if (canvas != null)
                Object.Destroy(canvas.gameObject);
        }
    }
}
