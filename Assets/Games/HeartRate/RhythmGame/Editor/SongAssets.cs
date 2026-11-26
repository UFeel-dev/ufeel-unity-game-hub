using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GameHub.RhythmGame.Editor
{
    public static class SongAssets
    {
        private const string AudioFolder = "Assets/Games/HeartRate/RhythmGame/Audio";
        private const string SongFolder = "Assets/Games/HeartRate/RhythmGame/Songs";

        private const string LibraryPath = SongFolder + "/SongLibrary.asset";

        public static AudioClip? ImportFile(string sourcePath)
        {
            string destination = AssetDatabase.GenerateUniqueAssetPath(AudioFolder + "/" + Path.GetFileName(sourcePath));
            File.Copy(sourcePath, destination);
            AssetDatabase.ImportAsset(destination);
            return AssetDatabase.LoadAssetAtPath<AudioClip>(destination);
        }

        public static RhythmSong? FindForClip(AudioClip clip)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:" + nameof(RhythmSong)))
            {
                RhythmSong song = AssetDatabase.LoadAssetAtPath<RhythmSong>(AssetDatabase.GUIDToAssetPath(guid));
                if (song != null && song.Clip == clip)
                    return song;
            }
            return null;
        }

        public static RhythmSong Save(RhythmSong? existing, SongDraft draft)
        {
            RhythmSong song = existing != null ? existing : CreateAsset(draft.Title);
            SerializedObject serialized = new(song);
            serialized.FindProperty("clip").objectReferenceValue = draft.Clip;
            serialized.FindProperty("title").stringValue = draft.Title;
            serialized.FindProperty("artist").stringValue = draft.Artist;
            serialized.FindProperty("bpm").floatValue = draft.Bpm;
            serialized.FindProperty("firstBeatOffset").floatValue = draft.FirstBeat;
            serialized.FindProperty("previewStart").floatValue = draft.PreviewStart;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(song);
            AddToLibrary(song);
            AssetDatabase.SaveAssets();
            return song;
        }

        private static RhythmSong CreateAsset(string title)
        {
            RhythmSong song = ScriptableObject.CreateInstance<RhythmSong>();
            AssetDatabase.CreateAsset(song, AssetDatabase.GenerateUniqueAssetPath(SongFolder + "/" + FileName(title) + ".asset"));
            return song;
        }

        private static void AddToLibrary(RhythmSong song)
        {
            RhythmSongLibrary library = AssetDatabase.LoadAssetAtPath<RhythmSongLibrary>(LibraryPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<RhythmSongLibrary>();
                AssetDatabase.CreateAsset(library, LibraryPath);
            }

            SerializedObject serialized = new(library);
            SerializedProperty songs = serialized.FindProperty("songs");
            for (int i = 0; i < songs.arraySize; i++)
            {
                if (songs.GetArrayElementAtIndex(i).objectReferenceValue == song)
                    return;
            }

            songs.InsertArrayElementAtIndex(songs.arraySize);
            songs.GetArrayElementAtIndex(songs.arraySize - 1).objectReferenceValue = song;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(library);
        }

        private static string FileName(string title)
        {
            StringBuilder builder = new();
            foreach (char character in title)
            {
                if (char.IsLetterOrDigit(character))
                    builder.Append(character);
                else if (builder.Length > 0 && builder[^1] != ' ')
                    builder.Append(' ');
            }
            string name = builder.ToString().Trim();
            return name.Length > 0 ? name : "Song";
        }
    }
}
