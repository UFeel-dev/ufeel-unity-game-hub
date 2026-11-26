using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.RhythmGame
{
    public sealed class HeartView
    {
        private const float IconSize = 34f;
        private const float PulseScale = 0.22f;
        private const float TraceWidth = 240f;
        private const float TraceHeight = 48f;
        private const float FallbackBpm = 60f;

        private readonly Image icon;
        private readonly TextMeshProUGUI bpm;
        private readonly TextMeshProUGUI caption;
        private readonly EcgTrace? trace;

        private int shownBpm = -1;
        private bool shownReading = true;

        public HeartView(UiKit kit, Transform parent, bool withTrace)
        {
            Root = UiKit.NewRect("Heart", parent);
            Root.sizeDelta = new Vector2(TraceWidth, withTrace ? 140f : 80f);

            icon = UiKit.NewImage("Icon", Root, kit.Sprites.Heart, Theme.Accent);
            UiLayout.Place(icon.rectTransform, new Vector2(0f, 1f), new Vector2(0.5f, 0.5f), new Vector2(IconSize / 2f, -24f), Vector2.one * IconSize);
            bpm = kit.NewText("Bpm", Root, 40f, true, TextAlignmentOptions.MidlineLeft, Theme.Text);
            UiLayout.Place(bpm.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(IconSize + 14f, -24f), new Vector2(160f, 48f));
            caption = kit.NewText("Caption", Root, Theme.Small, false, TextAlignmentOptions.MidlineLeft, Theme.TextDim);
            UiLayout.Place(caption.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(0f, -64f), new Vector2(TraceWidth, 28f));

            if (!withTrace)
                return;

            RectTransform traceArea = UiKit.NewRect("Trace", Root);
            UiLayout.Place(traceArea, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -88f), new Vector2(TraceWidth, TraceHeight));
            trace = new EcgTrace(traceArea, kit.Sprites);
        }

        public RectTransform Root { get; }

        public void Tick(float deltaTime, float value, bool hasReading, float pulse, Color color, string captionText, bool flat)
        {
            int rounded = Mathf.RoundToInt(value);
            if (hasReading != shownReading || (hasReading && rounded != shownBpm))
            {
                shownReading = hasReading;
                shownBpm = rounded;
                if (hasReading)
                    bpm.SetText("{0}", rounded);
                else
                    bpm.SetText("--");
            }

            if (caption.text != captionText)
                caption.text = captionText;

            icon.color = flat ? Theme.Danger : color;
            icon.rectTransform.localScale = Vector3.one * (1f + (PulseScale * (flat ? 0f : pulse)));
            if (trace == null)
                return;

            trace.SetColor(flat ? Theme.Danger : color);
            trace.Advance(deltaTime, hasReading ? value : FallbackBpm, flat);
        }
    }
}
