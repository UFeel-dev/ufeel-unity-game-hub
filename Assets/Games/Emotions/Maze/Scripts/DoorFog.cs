using GameHub.Common;
using TMPro;
using UnityEngine;

namespace GameHub.Maze
{
    [RequireComponent(typeof(Renderer), typeof(Collider))]
    public class DoorFog : MonoBehaviour
    {
        private const float ClosedAlpha = 0.7f;
        private const float ChargedAlpha = 0.3f;
        private const float OpenAlpha = 0.05f;
        private const float AlphaSpeed = 2f;
        private const float PulseAmplitude = 0.12f;
        private const float PulseSpeed = 5f;
        private const float ChargeGlow = 0.4f;
        private const string OpenCaption = "Open";
        private const string NoEntryCaption = "No entry";

        private static readonly Color LockedColor = new(0.3f, 0.3f, 0.33f, 1f);

        private Material material;
        private Collider blocker;
        private TMP_Text label;
        private Color color = LockedColor;
        private string caption = NoEntryCaption;
        private string? note;
        private float progress;
        private float alpha = ClosedAlpha;
        private bool usable;
        private bool open;
        private bool hinted;

        private void Awake()
        {
            material = GetComponent<Renderer>().material;
            blocker = GetComponent<Collider>();
            label = GetComponentInChildren<TMP_Text>(true);
            WorldText.Style(label, true, Theme.Backdrop);
        }

        private void OnDestroy()
        {
            Destroy(material);
        }

        private void Update()
        {
            float target = open ? OpenAlpha : Mathf.Lerp(ClosedAlpha, ChargedAlpha, progress);
            alpha = Mathf.MoveTowards(alpha, target, AlphaSpeed * Time.deltaTime);

            float pulse = open || progress <= 0f ? 0f : Mathf.Sin(Time.time * PulseSpeed) * PulseAmplitude * progress;
            Color tint = open ? color : Color.Lerp(color, Color.white, progress * ChargeGlow);
            material.color = Theme.Fade(tint, Mathf.Clamp01(alpha + pulse));
        }

        public void Setup(EmotionInfo emotion, PassageKind kind, bool usableFromHere)
        {
            usable = usableFromHere;
            color = usable ? emotion.Color : LockedColor;
            caption = usable ? emotion.Emotion.ToString() : NoEntryCaption;
            note = kind switch
            {
                PassageKind.OneWay when usable => "One way",
                PassageKind.ClosesBehind => "Closes behind you",
                _ => null,
            };
            RefreshLabel();
        }

        public void SetProgress(float value)
        {
            progress = usable ? Mathf.Clamp01(value) : 0f;
        }

        public void SetOpen(bool value)
        {
            open = value;
            blocker.enabled = !value;
            RefreshLabel();
        }

        public void SetHint(bool value)
        {
            if (hinted == value)
                return;

            hinted = value;
            RefreshLabel();
        }

        private void RefreshLabel()
        {
            string detail = hinted ? "Way out" : note ?? string.Empty;
            string text = open ? OpenCaption : caption;
            label.text = detail.Length == 0 ? text : $"{text}\n<size=55%>{detail}</size>";
        }
    }
}
