using GameHub.Common;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.RhythmGame
{
    public sealed class ReceptorView
    {
        private const float Size = 104f;
        private const float BeamHeight = 460f;
        private const float BeamWidth = 112f;
        private const float BurstSeconds = 0.2f;
        private const float BurstScale = 1.9f;
        private const float PulseScale = 0.06f;
        private const float PressSpeed = 30f;
        private const float ReleaseSpeed = 8f;

        private readonly RectTransform root;
        private readonly Image beam;
        private readonly Image fill;
        private readonly Image ring;
        private readonly Image burst;
        private readonly Color color;
        private readonly int lane;

        private float press;
        private float burstTime;
        private Color burstColor;

        public ReceptorView(Transform beamLayer, Transform receptorLayer, SpriteLibrary sprites, int lane, float x, float y)
        {
            color = RhythmTheme.Lane(lane);
            this.lane = lane;

            beam = UiKit.NewImage("Beam", beamLayer, sprites.Fade, Color.clear);
            UiLayout.Place(beam.rectTransform, Vector2.zero, new Vector2(0.5f, 0f), new Vector2(x, y), new Vector2(BeamWidth, BeamHeight));

            root = UiKit.NewRect("Receptor", receptorLayer);
            UiLayout.Place(root, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(x, y), new Vector2(Size, Size));
            fill = UiKit.NewImage("Fill", root, sprites.Circle, Color.clear);
            UiLayout.Stretch(fill.rectTransform);
            ring = UiKit.NewImage("Ring", root, sprites.Ring, Theme.Fade(color, 0.55f));
            UiLayout.Stretch(ring.rectTransform);
            burst = UiKit.NewImage("Burst", root, sprites.Glow, Color.clear);
            UiLayout.Stretch(burst.rectTransform);
        }

        public void SetStyle(NoteStyle style, SpriteLibrary sprites)
        {
            bool arrows = style == NoteStyle.Arrows;
            fill.sprite = arrows ? sprites.Arrow : sprites.Circle;
            ring.sprite = arrows ? sprites.ArrowOutline : sprites.Ring;
            Quaternion rotation = Quaternion.Euler(0f, 0f, arrows ? NoteView.ArrowAngle(lane) : 0f);
            fill.rectTransform.localRotation = rotation;
            ring.rectTransform.localRotation = rotation;
        }

        public void Burst(Color tint)
        {
            burstColor = tint;
            burstTime = BurstSeconds;
        }

        public void Tick(float deltaTime, bool held, float pulse)
        {
            float target = held ? 1f : 0f;
            press = Mathf.MoveTowards(press, target, deltaTime * (held ? PressSpeed : ReleaseSpeed));
            fill.color = Theme.Fade(color, 0.28f * press);
            beam.color = Theme.Fade(color, 0.22f * press);
            ring.color = Theme.Fade(Color.Lerp(color, Color.white, press * 0.6f), 0.5f + (0.4f * Mathf.Max(press, pulse * 0.5f)));
            root.localScale = Vector3.one * (1f + (PulseScale * pulse) - (0.06f * press));

            burstTime = Mathf.Max(0f, burstTime - deltaTime);
            float progress = 1f - (burstTime / BurstSeconds);
            burst.color = burstTime > 0f ? Theme.Fade(burstColor, 0.8f * (1f - progress)) : Color.clear;
            burst.rectTransform.localScale = Vector3.one * Mathf.Lerp(1f, BurstScale, progress);
        }
    }
}
