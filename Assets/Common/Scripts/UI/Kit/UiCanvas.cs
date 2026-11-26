using UnityEngine;
using UnityEngine.UI;

namespace GameHub.Common
{
    public static class UiCanvas
    {
        public const int HubOrder = 10;
        public const int GameOrder = 50;
        public const int FullScreenGameOrder = 100;
        public const int PauseOrder = 500;
        public const int SystemOrder = 1000;

        private static readonly Vector2 ReferenceResolution = new(1920f, 1080f);

        public static RectTransform Create(string name, Transform? parent, int sortingOrder)
        {
            GameObject root = new(name, typeof(RectTransform)) { layer = UiKit.UiLayer };
            root.transform.SetParent(parent, false);

            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent;

            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;

            if (sortingOrder < PauseOrder)
                root.AddComponent<MenuCover>();

            return (RectTransform)root.transform;
        }
    }
}
