#nullable enable

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UFeel
{
    internal sealed class UFeelFrameCapture : IFrameObserver
    {
        private readonly object gate = new();
        private byte[] pixels = Array.Empty<byte>();
        private int frameWidth;
        private int frameHeight;
        private long frameVersion;
        private long uploadedVersion;

        private volatile bool wanted;

        public bool IsWanted
        {
            get => wanted;
            set => wanted = value;
        }

        void IFrameObserver.OnFrameCaptured(UFeelProcessor processor)
        {
            if (!wanted || !processor.TryGetFrameSize(out int width, out int height))
                return;

            lock (gate)
            {
                int byteCount = checked(width * height * 4);
                if (pixels.Length != byteCount)
                    pixels = new byte[byteCount];

                GCHandle handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
                try
                {
                    if (!processor.TryCopyFrameBgra(handle.AddrOfPinnedObject(), width * 4, width, height))
                        return;
                }
                finally
                {
                    handle.Free();
                }

                frameWidth = width;
                frameHeight = height;
                frameVersion++;
            }
        }

        public Texture2D? Upload(Texture2D? texture)
        {
            lock (gate)
            {
                if (frameVersion == 0 || frameVersion == uploadedVersion)
                    return texture;

                if (texture == null || texture.width != frameWidth || texture.height != frameHeight)
                {
                    if (texture != null)
                        UnityEngine.Object.Destroy(texture);

                    texture = new Texture2D(frameWidth, frameHeight, TextureFormat.BGRA32, false) { name = "UFeelCameraFeed" };
                }

                texture.LoadRawTextureData(pixels);
                texture.Apply(false);
                uploadedVersion = frameVersion;
                return texture;
            }
        }

        public void Reset()
        {
            lock (gate)
                uploadedVersion = 0;
        }
    }
}
