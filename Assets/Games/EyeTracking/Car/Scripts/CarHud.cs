using GameHub.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.Car
{
    public sealed class CarHud
    {
        private const float Width = 440f;
        private const float RowHeight = 40f;
        private const float ArrowSize = 22f;
        private const float SideArrowX = 110f;
        private const float Bottom = 84f;
        private const float ShadeDepth = 320f;
        private const float ShadeOpacity = 0.6f;
        private const float MarkerSize = 36f;
        private const float MarkerOpacity = 0.5f;
        private const float MarkerFollow = 8f;
        private const string DirectionInstructions = "Look up or down to shift, left or right to steer";
        private const string ScreenInstructions = "Look where you want to go: higher is faster, lower reverses";

        private static readonly string[] GearLabels = { "Drive", "Stop", "Reverse" };

        private readonly RectTransform canvas;
        private readonly TextMeshProUGUI[] gears = new TextMeshProUGUI[GearLabels.Length];
        private readonly Image up;
        private readonly Image down;
        private readonly Image left;
        private readonly Image right;
        private readonly TextMeshProUGUI caption;
        private readonly Image marker;
        private Vector2 markerAnchor = new(0.5f, 0.5f);

        public CarHud(UiKit kit, Transform owner)
        {
            canvas = UiCanvas.Create("CarHud", owner, UiCanvas.GameOrder);
            kit.NewShade("Shade", canvas, RectTransform.Edge.Bottom, ShadeDepth, ShadeOpacity);

            RectTransform pad = UiKit.NewRect("Gears", canvas);
            float height = (GearLabels.Length + 2) * RowHeight;
            UiLayout.Place(pad, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-Theme.Margin, Bottom), new Vector2(Width, height + RowHeight));

            up = Arrow(kit, pad, 0f, new Vector2(0f, -RowHeight / 2f));
            for (int i = 0; i < GearLabels.Length; i++)
            {
                gears[i] = kit.NewText(GearLabels[i], pad, Theme.Heading, true, TextAlignmentOptions.Center, Theme.TextFaint);
                UiLayout.Row(gears[i].rectTransform, (i + 1) * RowHeight, RowHeight);
                gears[i].text = GearLabels[i];
            }

            down = Arrow(kit, pad, 180f, new Vector2(0f, -(GearLabels.Length + 1.5f) * RowHeight));
            float middle = -(Row(Gear.Stop) + 1.5f) * RowHeight;
            left = Arrow(kit, pad, 90f, new Vector2(-SideArrowX, middle));
            right = Arrow(kit, pad, -90f, new Vector2(SideArrowX, middle));

            caption = kit.NewText("Caption", pad, Theme.Tiny, false, TextAlignmentOptions.Center, Theme.TextDim);
            UiLayout.Row(caption.rectTransform, height, RowHeight);

            marker = UiKit.NewImage("Gaze", canvas, kit.Sprites.Ring, Theme.Fade(Theme.Accent, MarkerOpacity));
            marker.rectTransform.sizeDelta = Vector2.one * MarkerSize;

            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            canvas.gameObject.SetActive(visible);
        }

        public void SetMode(bool directions)
        {
            caption.text = directions ? DirectionInstructions : ScreenInstructions;
        }

        public void Set(Gear gear, int shiftDirection, float shiftProgress, int steering, Vector2? gaze, float deltaTime)
        {
            bool wasShown = marker.gameObject.activeSelf;
            marker.gameObject.SetActive(gaze.HasValue);
            if (gaze.HasValue)
            {
                Vector2 target = new(0.5f + (gaze.Value.x * 0.5f), 0.5f - (gaze.Value.y * 0.5f));
                markerAnchor = wasShown ? Vector2.Lerp(markerAnchor, target, 1f - Mathf.Exp(-MarkerFollow * deltaTime)) : target;
                marker.rectTransform.anchorMin = markerAnchor;
                marker.rectTransform.anchorMax = markerAnchor;
                marker.rectTransform.anchoredPosition = Vector2.zero;
            }

            for (int i = 0; i < gears.Length; i++)
                gears[i].color = i == Row(gear) ? Theme.Accent : Theme.TextFaint;

            up.color = ArrowColor(shiftDirection > 0, shiftProgress);
            down.color = ArrowColor(shiftDirection < 0, shiftProgress);
            left.color = steering < 0 ? Theme.Accent : Theme.TextFaint;
            right.color = steering > 0 ? Theme.Accent : Theme.TextFaint;
        }

        private static int Row(Gear gear)
        {
            return (int)Gear.Drive - (int)gear;
        }

        private static Color ArrowColor(bool active, float progress)
        {
            return active ? Color.Lerp(Theme.Text, Theme.Accent, progress) : Theme.TextFaint;
        }

        private static Image Arrow(UiKit kit, RectTransform parent, float angle, Vector2 position)
        {
            Image arrow = UiKit.NewImage("Arrow", parent, kit.Sprites.Triangle, Theme.TextFaint);
            UiLayout.Place(arrow.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), position, Vector2.one * ArrowSize);
            arrow.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
            return arrow;
        }
    }
}
