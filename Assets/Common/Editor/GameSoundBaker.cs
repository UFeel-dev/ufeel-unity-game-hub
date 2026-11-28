using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameHub.Common.Editor
{
    public static class GameSoundBaker
    {
        private const string CueFolder = "Assets/Common/Resources/Audio/Cues";
        private const string MusicFolder = "Assets/Common/Resources/Music";
        private const string StepFolder = "Assets/Common/Resources/Audio/Steps";
        private const string EscapeRoomTv = "Assets/Games/SpeechToText/EscapeRoom/Audio/Tv.wav";

        private static readonly (SoundCue Cue, Func<float[]> Synthesize)[] Cues =
        {
            (SoundCue.MenuMove, GameSynth.MenuMove),
            (SoundCue.MenuConfirm, GameSynth.MenuConfirm),
            (SoundCue.MenuBack, GameSynth.MenuBack),
            (SoundCue.Start, GameSynth.Start),
            (SoundCue.Success, GameSynth.Success),
            (SoundCue.Fail, GameSynth.Fail),
            (SoundCue.Win, GameSynth.Win),
            (SoundCue.Shot, GameSynth.Shot),
            (SoundCue.Hit, GameSynth.Hit),
            (SoundCue.Hurt, GameSynth.Hurt),
            (SoundCue.Engine, GameSynth.Engine),
            (SoundCue.Charge, GameSynth.Charge),
        };

        private static readonly (string Scene, Func<float[]> Synthesize)[] Music =
        {
            ("GameHub", GameSynth.HubMusic),
            ("Maze", GameSynth.MazeMusic),
            ("Animals", GameSynth.AnimalsMusic),
            ("Car", GameSynth.CarMusic),
            ("Survivor", GameSynth.SurvivorMusic),
            ("HorrorRoom", GameSynth.HorrorMusic),
            ("EscapeRoom", GameSynth.EscapeRoomMusic),
        };

        [MenuItem("Tools/Common/Bake Sounds")]
        public static void Bake()
        {
            Directory.CreateDirectory(CueFolder);
            Directory.CreateDirectory(MusicFolder);
            Directory.CreateDirectory(StepFolder + "/Default");
            Directory.CreateDirectory(StepFolder + "/Maze");
            foreach ((SoundCue cue, Func<float[]> synthesize) in Cues)
                Write($"{CueFolder}/{cue}.wav", synthesize(), AudioClipLoadType.DecompressOnLoad);
            foreach ((string scene, Func<float[]> synthesize) in Music)
                Write($"{MusicFolder}/{scene}.wav", synthesize(), AudioClipLoadType.CompressedInMemory);
            Write(EscapeRoomTv, GameSynth.Tv(), AudioClipLoadType.DecompressOnLoad);
            for (int i = 0; i < GameSynth.StepVariants; i++)
            {
                Write($"{StepFolder}/Default/Step{i}.wav", GameSynth.DefaultStep(i), AudioClipLoadType.DecompressOnLoad);
                Write($"{StepFolder}/Maze/Step{i}.wav", GameSynth.MazeStep(i), AudioClipLoadType.DecompressOnLoad);
            }

            AssetDatabase.SaveAssets();
        }

        private static void Write(string path, float[] samples, AudioClipLoadType loadType)
        {
            ProceduralAudio.WriteWav(path, samples);
            AssetDatabase.ImportAsset(path);

            AudioImporter importer = (AudioImporter)AssetImporter.GetAtPath(path);
            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            settings.loadType = loadType;
            settings.compressionFormat = loadType == AudioClipLoadType.DecompressOnLoad ? AudioCompressionFormat.PCM : AudioCompressionFormat.Vorbis;
            settings.preloadAudioData = true;
            importer.defaultSampleSettings = settings;
            importer.SaveAndReimport();
        }
    }
}
