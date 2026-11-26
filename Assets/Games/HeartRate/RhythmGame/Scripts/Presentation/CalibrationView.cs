using System.Collections.Generic;
using GameHub.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.RhythmGame
{
    public sealed class CalibrationView
    {
        private const float RingSize = 180f;
        private const float DotSize = 12f;
        private const float DotGap = 22f;

        private readonly RectTransform root;
        private readonly Image ring;
        private readonly Image core;
        private readonly TextMeshProUGUI result;
        private readonly List<Image> dots = new();

        public CalibrationView(UiKit kit, Transform parent, int taps)
        {
            root = UiKit.NewRect("Calibration", parent);
            UiLayout.Stretch(root);

            TextMeshProUGUI title = kit.NewText("Title", root, Theme.Title, true, TextAlignmentOptions.Center, Theme.Text);
            UiLayout.Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 300f), new Vector2(900f, 60f));
            title.text = "Audio offset";
            TextMeshProUGUI hint = kit.NewText("Hint", root, Theme.Body, false, TextAlignmentOptions.Center, Theme.TextDim);
            UiLayout.Place(hint.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 248f), new Vector2(900f, 36f));
            hint.text = "Tap a lane key on every click.";

            ring = UiKit.NewImage("Ring", root, kit.Sprites.Ring, Theme.Text);
            UiLayout.Place(ring.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), Vector2.one * RingSize);
            core = UiKit.NewImage("Core", root, kit.Sprites.Circle, Theme.Accent);
            UiLayout.Place(core.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), Vector2.one * (RingSize * 0.5f));

            RectTransform row = UiKit.NewRect("Progress", root);
            UiLayout.Place(row, new Vector2(0.5f, 0.5f), new Vector2(0f, -110f), new Vector2(taps * DotGap, DotSize));
            for (int i = 0; i < taps; i++)
            {
                Image dot = UiKit.NewImage("Dot", row, kit.Sprites.Circle, Theme.Line);
                UiLayout.Place(dot.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(i * DotGap, 0f), Vector2.one * DotSize);
                dots.Add(dot);
            }

            result = kit.NewText("Result", root, Theme.Display, true, TextAlignmentOptions.Center, Theme.Text);
            UiLayout.Place(result.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -200f), new Vector2(600f, 90f));

            root.gameObject.SetActive(false);
        }

        public void SetVisible(bool visible)
        {
            root.gameObject.SetActive(visible);
        }

        public void SetBeat(float pulse)
        {
            ring.rectTransform.localScale = Vector3.one * (1f + (0.12f * pulse));
            ring.color = Theme.Fade(Theme.Text, 0.25f + (0.6f * pulse));
            core.color = Theme.Fade(Theme.Accent, 0.15f + (0.7f * pulse));
        }

        public void SetProgress(int taps)
        {
            for (int i = 0; i < dots.Count; i++)
                dots[i].color = i < taps ? Theme.Accent : Theme.Line;
        }

        public void SetResult(string text)
        {
            result.text = text;
        }
    }
}
