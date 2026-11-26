using UnityEngine;
using UnityEngine.UI;

namespace GameHub.Common
{
    public sealed class Scrim
    {
        private readonly Image image;
        private readonly float opacity;
        private readonly Fader fader = new();

        public Scrim(UiKit kit, Transform parent, float opacity)
        {
            this.opacity = opacity;
            image = kit.NewSolid("Scrim", parent, Color.clear);
            UiLayout.Stretch(image.rectTransform);
            image.enabled = false;
        }

        public void Show(bool visible, float fadeSeconds = 0f)
        {
            fader.To(visible, fadeSeconds);
            Apply();
        }

        public void Tick(float deltaTime)
        {
            fader.Tick(deltaTime);
            Apply();
        }

        private void Apply()
        {
            image.enabled = fader.Value > 0f;
            image.color = Theme.Fade(Theme.Backdrop, opacity * fader.Value);
        }
    }
}
