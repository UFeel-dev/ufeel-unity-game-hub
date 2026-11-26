using GameHub.Common;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.RhythmGame
{
    public sealed class BackdropView
    {
        private const float MenuDim = 0.72f;
        private const float PlayDim = 0.5f;
        private const float DimSpeed = 3f;
        private const float VignetteAlpha = 0.7f;
        private const float TintShare = 0.12f;
        private const float TintPulse = 0.08f;
        private const float FlashBase = 0.35f;
        private const float FlashPulse = 0.25f;

        private readonly Image dim;
        private readonly Image vignette;
        private readonly Image flash;

        private float targetDim = MenuDim;

        public BackdropView(Transform parent, SpriteLibrary sprites)
        {
            dim = UiKit.NewImage("Dim", parent, sprites.Solid, Theme.Fade(Theme.Backdrop, MenuDim));
            UiLayout.Stretch(dim.rectTransform);
            vignette = UiKit.NewImage("Vignette", parent, sprites.Vignette, Color.clear);
            UiLayout.Stretch(vignette.rectTransform);
            flash = UiKit.NewImage("Flash", parent, sprites.Vignette, Color.clear);
            UiLayout.Stretch(flash.rectTransform);
        }

        public void SetPlaying(bool playing)
        {
            targetDim = playing ? PlayDim : MenuDim;
        }

        public void Tick(float deltaTime, Color tint, float pulse, float flashIntensity)
        {
            float alpha = Mathf.MoveTowards(dim.color.a, targetDim, deltaTime * DimSpeed);
            dim.color = Theme.Fade(Theme.Backdrop, alpha);
            vignette.color = Theme.Fade(Color.Lerp(Theme.Backdrop, tint, TintShare + (TintPulse * pulse)), VignetteAlpha);
            flash.color = Theme.Fade(tint, flashIntensity * (FlashBase + (FlashPulse * pulse)));
        }
    }
}
