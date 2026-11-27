using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameHub.Maze.Editor
{
    public static class MazeSoundBaker
    {
        private const string AudioFolder = "Assets/Games/Emotions/Maze/Audio";
        private const string SoundSetPath = AudioFolder + "/MazeSounds.asset";

        private static readonly (string Field, string File, Func<float[]> Synthesize)[] Sounds =
        {
            ("ambient", "MazeAmbient", MazeSynth.Ambient),
            ("charge", "MazeCharge", MazeSynth.Charge),
            ("doorOpen", "MazeDoorOpen", MazeSynth.DoorOpen),
            ("doorClose", "MazeDoorClose", MazeSynth.DoorClose),
            ("hint", "MazeHint", MazeSynth.Hint),
            ("bonus", "MazeBonus", MazeSynth.Bonus),
            ("trap", "MazeTrap", MazeSynth.Trap),
            ("win", "MazeWin", MazeSynth.Win),
        };

        [MenuItem("Tools/Maze/Bake Sounds")]
        public static void Bake()
        {
            Directory.CreateDirectory(AudioFolder);
            MazeSoundSet set = AssetDatabase.LoadAssetAtPath<MazeSoundSet>(SoundSetPath);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<MazeSoundSet>();
                AssetDatabase.CreateAsset(set, SoundSetPath);
            }

            SerializedObject serialized = new(set);
            foreach ((string field, string file, Func<float[]> synthesize) in Sounds)
                serialized.FindProperty(field).objectReferenceValue = Write(file, synthesize());

            List<AudioClip> steps = new();
            for (int i = 0; i < MazeSynth.StepVariants; i++)
                steps.Add(Write($"MazeStep{i}", MazeSynth.Step(i)));

            SerializedProperty stepList = serialized.FindProperty("steps");
            stepList.arraySize = steps.Count;
            for (int i = 0; i < steps.Count; i++)
                stepList.GetArrayElementAtIndex(i).objectReferenceValue = steps[i];

            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
        }

        private static AudioClip Write(string file, float[] samples)
        {
            string path = $"{AudioFolder}/{file}.wav";
            MazeSynth.WriteWav(path, samples);
            AssetDatabase.ImportAsset(path);

            AudioImporter importer = (AudioImporter)AssetImporter.GetAtPath(path);
            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.PCM;
            settings.preloadAudioData = true;
            importer.defaultSampleSettings = settings;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
    }
}
