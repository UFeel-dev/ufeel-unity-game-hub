using System;
using System.Collections.Generic;
using System.IO;
using GameHub.Common.Editor;
using UnityEditor;
using UnityEngine;

namespace GameHub.Hub.Editor
{
    public static class HubSoundBaker
    {
        private const string AudioFolder = "Assets/Hub/Audio";
        private const string SoundSetPath = AudioFolder + "/HubSounds.asset";

        private static readonly (string Field, string File, Func<float[]> Synthesize)[] Sounds =
        {
            ("ambient", "HubAmbient", HubSynth.Ambient),
            ("menuMove", "HubMenuMove", HubSynth.MenuMove),
            ("menuConfirm", "HubMenuConfirm", HubSynth.MenuConfirm),
            ("menuBack", "HubMenuBack", HubSynth.MenuBack),
            ("carouselHum", "HubCarouselHum", HubSynth.CarouselHum),
        };

        [MenuItem("Tools/Hub/Bake Sounds")]
        public static void Bake()
        {
            Directory.CreateDirectory(AudioFolder);
            HubSoundSet set = AssetDatabase.LoadAssetAtPath<HubSoundSet>(SoundSetPath);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<HubSoundSet>();
                AssetDatabase.CreateAsset(set, SoundSetPath);
            }

            SerializedObject serialized = new(set);
            foreach ((string field, string file, Func<float[]> synthesize) in Sounds)
                serialized.FindProperty(field).objectReferenceValue = Write(file, synthesize());

            List<AudioClip> steps = new();
            for (int i = 0; i < HubSynth.StepVariants; i++)
                steps.Add(Write($"HubStep{i}", HubSynth.Step(i)));

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
            ProceduralAudio.WriteWav(path, samples);
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
