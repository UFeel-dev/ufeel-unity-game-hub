using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;

namespace GameHub.RhythmGame
{
    public sealed class OnsetAnalyzer
    {
        private const int ChunkFrames = 16384;
        private const float ReadShare = 0.9f;

        private readonly AudioClip clip;
        private readonly float[] buffer;
        private readonly OnsetCore core;

        private int readFrames;
        private Task<ChartSet>? finishing;

        public OnsetAnalyzer(RhythmSong song)
        {
            Song = song;
            clip = song.Clip;
            buffer = new float[ChunkFrames * Mathf.Max(1, clip.channels)];
            core = new OnsetCore(clip.frequency, clip.samples);
        }

        public RhythmSong Song { get; }

        public ChartSet? Result { get; private set; }

        public bool Failed { get; private set; }

        public float Progress => Result != null ? 1f : ReadShare * readFrames / Mathf.Max(1, clip.samples);

        private bool IsDone => Result != null || Failed;

        public void Step(double budgetMilliseconds)
        {
            if (IsDone)
                return;
            if (finishing != null)
            {
                Collect(finishing);
                return;
            }
            if (!IsLoaded())
                return;

            Stopwatch watch = Stopwatch.StartNew();
            while (readFrames < clip.samples && watch.Elapsed.TotalMilliseconds < budgetMilliseconds)
            {
                if (!ReadChunk())
                    return;
            }

            if (readFrames >= clip.samples)
                StartFinish();
        }

        private bool IsLoaded()
        {
            if (clip.loadType != AudioClipLoadType.DecompressOnLoad)
                return Fail($"Rhythm song '{Song.name}' needs its clip imported with Load Type 'Decompress On Load'.");
            if (clip.loadState == AudioDataLoadState.Unloaded)
                clip.LoadAudioData();
            if (clip.loadState == AudioDataLoadState.Failed)
                return Fail($"Rhythm song '{Song.name}' audio failed to load.");
            return clip.loadState == AudioDataLoadState.Loaded;
        }

        private bool ReadChunk()
        {
            int frames = Mathf.Min(ChunkFrames, clip.samples - readFrames);
            if (!clip.GetData(buffer, readFrames))
                return Fail($"Rhythm song '{Song.name}' audio data could not be read.");

            core.Feed(buffer, frames, clip.channels);
            readFrames += frames;
            return true;
        }

        private void StartFinish()
        {
            OnsetCore finishingCore = core;
            GridFit hint = new(Song.Bpm, Song.FirstBeatOffset);
            double length = clip.length;
            int seed = StableSeed(Song.Id);
            finishing = Task.Run(() => ChartSetBuilder.Build(finishingCore.Finish(hint, length), PatternCatalogue.CreateDefault(), seed));
        }

        private void Collect(Task<ChartSet> task)
        {
            if (!task.IsCompleted)
                return;

            finishing = null;
            if (task.IsFaulted || task.IsCanceled)
            {
                if (task.Exception != null)
                    UnityEngine.Debug.LogException(task.Exception);
                Fail($"Rhythm song '{Song.name}' analysis failed.");
                return;
            }
            Result = task.Result;
        }

        private static int StableSeed(string text)
        {
            unchecked
            {
                int hash = (int)2166136261;
                foreach (char character in text)
                    hash = (hash ^ character) * 16777619;
                return hash & int.MaxValue;
            }
        }

        private bool Fail(string message)
        {
            if (!Failed)
                UnityEngine.Debug.LogError(message);
            Failed = true;
            return false;
        }
    }
}
