#nullable enable

using System;

namespace UFeel
{
    public sealed class FrameData
    {
        public FrameData(byte[] data, int width, int height, int channels, int stride)
        {
            Buffer = data;
            Width = width;
            Height = height;
            Channels = channels;
            Stride = stride;
        }

        public ReadOnlyMemory<byte> Data => Buffer;
        public int Width { get; }
        public int Height { get; }
        public int Channels { get; }
        public int Stride { get; }

        internal byte[] Buffer { get; }
    }
}
