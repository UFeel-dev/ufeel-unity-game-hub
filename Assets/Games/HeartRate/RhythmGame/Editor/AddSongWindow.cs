using System;
using UnityEditor;
using UnityEngine;

namespace GameHub.RhythmGame.Editor
{
    public sealed class AddSongWindow : EditorWindow
    {
        private const float PreviewSeconds = 15f;
        private const string ArtistSeparator = " - ";

        private readonly SongDraft draft = new();
        private RhythmSong? existing;
        private PendingAction pending;
        private string status = string.Empty;
        private MessageType statusType = MessageType.None;
        private GameObject? playerHost;
        private AudioClip? previewClip;

        [MenuItem("Tools/Rhythm Game/Add Song")]
        private static void Open()
        {
            Show(null);
        }

        [MenuItem("Assets/Add to Rhythm Game")]
        private static void OpenForSelection()
        {
            Show(Selection.activeObject as AudioClip);
        }

        [MenuItem("Assets/Add to Rhythm Game", true)]
        private static bool CanOpenForSelection()
        {
            return Selection.activeObject is AudioClip;
        }

        private static void Show(AudioClip? clip)
        {
            AddSongWindow window = GetWindow<AddSongWindow>();
            window.titleContent = new GUIContent("Add Song");
            window.minSize = new Vector2(380f, 360f);
            window.Show();
            if (clip != null)
                window.SetClip(clip);
        }

        private void OnDisable()
        {
            StopPreview();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            DrawAudio();
            using (new EditorGUI.DisabledScope(draft.Clip == null))
            {
                EditorGUILayout.Space();
                draft.Title = EditorGUILayout.TextField("Title", draft.Title);
                draft.Artist = EditorGUILayout.TextField("Artist", draft.Artist);
                EditorGUILayout.Space();
                DrawTiming();
                EditorGUILayout.Space();
                DrawPreview();
            }

            if (!string.IsNullOrEmpty(status))
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(status, statusType);
            }

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(draft.Clip == null || draft.Bpm <= 0f))
            {
                if (GUILayout.Button(existing != null ? "Update song" : "Add song", GUILayout.Height(30f)))
                    Save();
            }
        }

        private void DrawAudio()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                AudioClip? picked = (AudioClip?)EditorGUILayout.ObjectField("Audio", draft.Clip, typeof(AudioClip), false);
                if (picked != draft.Clip && picked != null)
                    SetClip(picked);
                if (GUILayout.Button("Import file...", GUILayout.Width(100f)))
                    ImportFile();
            }
        }

        private void DrawTiming()
        {
            draft.Bpm = EditorGUILayout.FloatField("BPM", draft.Bpm);
            draft.FirstBeat = EditorGUILayout.FloatField("First beat (s)", draft.FirstBeat);
            draft.PreviewStart = EditorGUILayout.FloatField("Preview start (s)", draft.PreviewStart);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(EditorGUIUtility.labelWidth);
                if (GUILayout.Button("Detect"))
                    Detect();
                if (GUILayout.Button("x 2"))
                    draft.Bpm *= 2f;
                if (GUILayout.Button("/ 2"))
                    draft.Bpm /= 2f;
                if (GUILayout.Button("1/2 beat"))
                    ShiftHalfBeat();
            }
        }

        private void DrawPreview()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(EditorGUIUtility.labelWidth);
                if (GUILayout.Button("Listen with clicks"))
                    Listen();
                if (GUILayout.Button("Stop", GUILayout.Width(60f)))
                    StopPreview();
            }
            EditorGUILayout.LabelField(" ", "Clicks should land on the beat, the loud one on the first beat of each bar.", EditorStyles.wordWrappedMiniLabel);
        }

        private void SetClip(AudioClip clip)
        {
            StopPreview();
            draft.Clip = clip;
            existing = SongAssets.FindForClip(clip);
            if (existing != null)
            {
                draft.Title = existing.Title;
                draft.Artist = existing.Artist;
                draft.Bpm = existing.Bpm;
                draft.FirstBeat = existing.FirstBeatOffset;
                draft.PreviewStart = existing.PreviewStart;
                SetStatus("This song is already in the game. Changes will update it.", MessageType.Info);
                return;
            }

            int separator = clip.name.IndexOf(ArtistSeparator, StringComparison.Ordinal);
            draft.Artist = separator > 0 ? clip.name[..separator].Trim() : string.Empty;
            draft.Title = separator > 0 ? clip.name[(separator + ArtistSeparator.Length)..].Trim() : clip.name;
            Detect();
        }

        private void ImportFile()
        {
            string path = EditorUtility.OpenFilePanel("Choose a song", string.Empty, "mp3,ogg,wav");
            if (string.IsNullOrEmpty(path))
                return;

            AudioClip? clip = SongAssets.ImportFile(path);
            if (clip == null)
            {
                SetStatus("Unity could not import this file.", MessageType.Error);
                return;
            }
            SetClip(clip);
        }

        private void Detect()
        {
            Queue(PendingAction.Detect);
        }

        private void Listen()
        {
            Queue(PendingAction.Listen);
        }

        private void Queue(PendingAction action)
        {
            if (draft.Clip == null || (action == PendingAction.Listen && draft.Bpm <= 0f))
                return;

            pending = action;
            SetStatus("Loading the audio...", MessageType.None);
        }

        private void Update()
        {
            if (pending == PendingAction.None || draft.Clip == null)
                return;

            if (SongAudio.HasFailed(draft.Clip))
            {
                pending = PendingAction.None;
                SetStatus("The audio could not be loaded.", MessageType.Error);
                Repaint();
                return;
            }

            if (!SongAudio.Request(draft.Clip))
                return;

            PendingAction action = pending;
            pending = PendingAction.None;
            if (action == PendingAction.Detect)
                RunDetection(draft.Clip);
            else
                PlayPreview(draft.Clip);
            Repaint();
        }

        private void RunDetection(AudioClip clip)
        {
            try
            {
                EditorUtility.DisplayProgressBar("Add Song", "Listening to the song...", 0.5f);
                SongDetection detection = SongAudio.Detect(clip);
                draft.Bpm = (float)Math.Round(detection.Bpm, 2);
                draft.FirstBeat = (float)Math.Round(detection.FirstBeat, 3);
                draft.PreviewStart = (float)Math.Round(detection.PreviewStart, 1);
                SetStatus("Detected. Listen to check the beat before adding the song.", MessageType.Info);
            }
            catch (InvalidOperationException exception)
            {
                SetStatus(exception.Message, MessageType.Error);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private void ShiftHalfBeat()
        {
            if (draft.Bpm <= 0f)
                return;

            float beat = 60f / draft.Bpm;
            draft.FirstBeat = (float)Math.Round((draft.FirstBeat + (beat / 2f)) % beat, 3);
        }

        private void PlayPreview(AudioClip clip)
        {
            StopPreview();
            SetStatus(string.Empty, MessageType.None);
            previewClip = SongAudio.PreviewWithClicks(clip, draft.PreviewStart, draft.Bpm, draft.FirstBeat, PreviewSeconds);
            playerHost = EditorUtility.CreateGameObjectWithHideFlags("Song preview", HideFlags.HideAndDontSave, typeof(AudioSource));
            AudioSource source = playerHost.GetComponent<AudioSource>();
            source.clip = previewClip;
            source.spatialBlend = 0f;
            source.Play();
        }

        private void StopPreview()
        {
            if (playerHost != null)
                DestroyImmediate(playerHost);
            if (previewClip != null)
                DestroyImmediate(previewClip);
            playerHost = null;
            previewClip = null;
        }

        private void Save()
        {
            if (draft.Clip == null)
                return;

            StopPreview();
            existing = SongAssets.Save(existing, draft);
            Selection.activeObject = existing;
            SetStatus($"{draft.Title} is in the song list.", MessageType.Info);
        }

        private void SetStatus(string message, MessageType type)
        {
            status = message;
            statusType = type;
        }

        private enum PendingAction
        {
            None,
            Detect,
            Listen,
        }
    }
}
