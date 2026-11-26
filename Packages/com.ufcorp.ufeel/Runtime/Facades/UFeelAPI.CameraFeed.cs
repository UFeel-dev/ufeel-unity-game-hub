#nullable enable

using UnityEngine;

namespace UFeel
{
    public sealed partial class UFeelAPI
    {
        public static class CameraFeed
        {
            private static Texture2D? frame;

            public static bool IsCapturing
            {
                get => FrameCapture.IsWanted;
                set => FrameCapture.IsWanted = value;
            }

            /// <remarks>Updated while <see cref="IsCapturing"/> is true. Top row first: flip it vertically to display it.</remarks>
            public static Texture2D? Frame => frame;

            internal static void Upload()
            {
                if (IsCapturing)
                    frame = FrameCapture.Upload(frame);
            }

            internal static void Release()
            {
                if (frame != null)
                    Destroy(frame);

                frame = null;
                FrameCapture.Reset();
            }
        }
    }
}
