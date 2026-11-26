using System;
using UnityEditor;
using UnityEngine;

namespace GameHub.RhythmGame.Editor
{
    public static class SongAudio
    {
        private const float ClickFrequency = 1800f;
        private const float ClickSeconds = 0.04f;
        private const float ClickVolume = 0.5f;
        private const float AccentVolume = 0.9f;
        private const float MusicVolume = 0.7f;

        public static bool Request(AudioClip clip)
        {
            PrepareImport(clip);
            if (clip.loadState is AudioDataLoadState.Unloaded or AudioDataLoadState.Failed)
                clip.LoadAudioData();
            return clip.loadState == AudioDataLoadState.Loaded;
        }

        public static bool HasFailed(AudioClip clip)
        {
            return clip.loadState == AudioDataLoadState.Failed;
        }

        public static SongDetection Detect(AudioClip clip)
        {
            float[] samples = Samples(clip, 0, clip.samples);
            OnsetCore core = new(clip.frequency, clip.samples);
            core.Feed(samples, clip.samples, clip.channels);
            return SongDetector.Detect(core.Curves(), clip.length);
        }

        public static AudioClip PreviewWithClicks(AudioClip clip, double start, double bpm, double firstBeat, double seconds)
        {
            int channels = clip.channels;
            int offset = Math.Clamp((int)(start * clip.frequency), 0, clip.samples - 1);
            int frames = Math.Min((int)(seconds * clip.frequency), clip.samples - offset);
            float[] data = Samples(clip, offset, frames);
            for (int i = 0; i < data.Length; i++)
                data[i] *= MusicVolume;

            double beat = 60d / bpm;
            long firstIndex = (long)Math.Ceiling((start - firstBeat) / beat);
            for (long index = firstIndex; firstBeat + (index * beat) < start + seconds; index++)
            {
                int at = (int)((firstBeat + (index * beat) - start) * clip.frequency);
                AddClick(data, at, frames, channels, clip.frequency, index % BeatGrid.BeatsPerMeasure == 0 ? AccentVolume : ClickVolume);
            }

            AudioClip preview = AudioClip.Create("Song preview", frames, channels, clip.frequency, false);
            preview.SetData(data, 0);
            return preview;
        }

        private static float[] Samples(AudioClip clip, int offset, int frames)
        {
            float[] data = new float[frames * clip.channels];
            if (clip.loadState != AudioDataLoadState.Loaded || !clip.GetData(data, offset))
                throw new InvalidOperationException($"The audio of '{clip.name}' could not be read.");
            return data;
        }

        private static void AddClick(float[] data, int start, int frames, int channels, int frequency, float volume)
        {
            int length = (int)(ClickSeconds * frequency);
            for (int i = 0; i < length && start + i < frames; i++)
            {
                if (start + i < 0)
                    continue;

                float time = i / (float)frequency;
                float value = volume * Mathf.Exp(-time * 120f) * Mathf.Sin(2f * Mathf.PI * ClickFrequency * time);
                for (int channel = 0; channel < channels; channel++)
                {
                    int index = ((start + i) * channels) + channel;
                    data[index] = Mathf.Clamp(data[index] + value, -1f, 1f);
                }
            }
        }

        private static void PrepareImport(AudioClip clip)
        {
            AudioImporter importer = (AudioImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(clip));
            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            if (settings.loadType == AudioClipLoadType.DecompressOnLoad && importer.loadInBackground)
                return;

            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            importer.defaultSampleSettings = settings;
            importer.loadInBackground = true;
            importer.SaveAndReimport();
        }
    }
}
