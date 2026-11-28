using System;
using static GameHub.Common.Editor.ProceduralAudio;

namespace GameHub.Hub.Editor
{
    public static class HubSynth
    {
        public static float[] CarouselHum()
        {
            return Wave(2f, 0.5f, t =>
            {
                float wobble = 0.8f + (0.2f * Sine(2f, t));
                return ((Sine(60f, t) * 0.5f) + (Sine(90f, t) * 0.3f) + (Sine(120f, t) * 0.2f) + (Sine(240f, t) * 0.05f)) * wobble;
            });
        }
    }
}
