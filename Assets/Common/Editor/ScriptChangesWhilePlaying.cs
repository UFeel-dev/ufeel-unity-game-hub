using UnityEditor;
using UnityEngine;

namespace GameHub.Common.Editor
{
    [InitializeOnLoad]
    internal static class ScriptChangesWhilePlaying
    {
        private const string PreferenceKey = "ScriptCompilationDuringPlay";
        private const int RecompileAndContinuePlaying = 0;
        private const int StopPlayingAndRecompile = 2;

        static ScriptChangesWhilePlaying()
        {
            if (EditorPrefs.GetInt(PreferenceKey, RecompileAndContinuePlaying) != RecompileAndContinuePlaying)
                return;

            EditorPrefs.SetInt(PreferenceKey, StopPlayingAndRecompile);
            Debug.Log("Script changes while playing now stop Play mode before recompiling (Preferences > General > Script Changes While Playing).");
        }
    }
}
