using UnityEngine;
using UnityEngine.UI;

namespace GameHub.RhythmGame
{
    public sealed class NoteView
    {
        public const float HeadSize = 100f;

        private const float GlowSize = 160f;
        private const float BodyWidth = 64f;
        private const float TailSize = 58f;
        private const float RingAlpha = 0.85f;
        private const float GlowAlpha = 0.35f;
        private const float BodyAlpha = 0.45f;
        private const float HeldBodyAlpha = 0.8f;

        private static readonly float[] ArrowAngles = { 90f, 180f, 0f, -90f };

        private readonly SpriteLibrary sprites;
        private readonly RectTransform root;
        private readonly Image body;
        private readonly Image tail;
        private readonly Image glow;
        private readonly Image head;
        private readonly Image ring;

        private NoteStyle style;
        private int shownLane = -1;
        private Color tint;
        private bool isHold;
        private bool isHolding;
        private bool isDimmed;

        public NoteView(Transform parent, SpriteLibrary sprites, NoteStyle style)
        {
            this.sprites = sprites;
            root = UiKit.NewRect("Note", parent);
            UiLayout.Place(root, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(HeadSize, HeadSize));

            body = UiKit.NewImage("Body", root, sprites.Pill, Color.white);
            body.type = Image.Type.Sliced;
            UiLayout.Place(body.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(BodyWidth, 0f));
            tail = UiKit.NewImage("Tail", root, sprites.Circle, Color.white);
            UiLayout.Place(tail.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(TailSize, TailSize));
            glow = UiKit.NewImage("Glow", root, sprites.Glow, Color.white);
            UiLayout.Place(glow.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(GlowSize, GlowSize));
            head = UiKit.NewImage("Head", root, sprites.Circle, Color.white);
            UiLayout.Place(head.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(HeadSize, HeadSize));
            ring = UiKit.NewImage("Ring", root, sprites.Ring, Color.white);
            UiLayout.Place(ring.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(HeadSize, HeadSize));

            SetHold(false);
            SetStyle(style);
            root.gameObject.SetActive(false);
        }

        public static float ArrowAngle(int lane)
        {
            return ArrowAngles[lane];
        }

        public void SetStyle(NoteStyle value)
        {
            style = value;
            shownLane = -1;
            head.sprite = value == NoteStyle.Arrows ? sprites.Arrow : sprites.Circle;
            ring.sprite = value == NoteStyle.Arrows ? sprites.ArrowOutline : sprites.Ring;
        }

        public void Show(float headY, float tailY, int lane, bool hold, bool holding, bool dimmed)
        {
            if (!root.gameObject.activeSelf)
                root.gameObject.SetActive(true);

            root.anchoredPosition = new Vector2(PlayfieldView.LaneCenter(lane), headY);
            if (lane != shownLane)
                Orient(lane);
            if (hold != isHold)
                SetHold(hold);
            if (hold)
            {
                float length = Mathf.Max(0f, tailY - headY);
                body.rectTransform.sizeDelta = new Vector2(BodyWidth, length);
                tail.rectTransform.anchoredPosition = new Vector2(0f, length);
            }

            Color color = Theme.Lane(lane);
            if (color != tint || holding != isHolding || dimmed != isDimmed)
                Recolor(color, holding, dimmed);
        }

        public void Hide()
        {
            if (root.gameObject.activeSelf)
                root.gameObject.SetActive(false);
        }

        private void Orient(int lane)
        {
            shownLane = lane;
            Quaternion rotation = Quaternion.Euler(0f, 0f, style == NoteStyle.Arrows ? ArrowAngle(lane) : 0f);
            head.rectTransform.localRotation = rotation;
            ring.rectTransform.localRotation = rotation;
        }

        private void SetHold(bool hold)
        {
            isHold = hold;
            body.gameObject.SetActive(hold);
            tail.gameObject.SetActive(hold);
        }

        private void Recolor(Color color, bool holding, bool dimmed)
        {
            tint = color;
            isHolding = holding;
            isDimmed = dimmed;

            head.color = dimmed ? Theme.Missed : color;
            ring.color = dimmed ? Theme.Missed : Theme.Fade(Color.white, RingAlpha);
            glow.color = dimmed ? Color.clear : Theme.Fade(color, holding ? GlowAlpha * 1.6f : GlowAlpha);
            body.color = dimmed ? Theme.Missed : Theme.Fade(color, holding ? HeldBodyAlpha : BodyAlpha);
            tail.color = dimmed ? Theme.Missed : Theme.Fade(color, 0.9f);
        }
    }
}
