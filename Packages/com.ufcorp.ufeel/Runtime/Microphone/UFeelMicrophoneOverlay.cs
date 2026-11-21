#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

namespace UFeel
{
    internal sealed class UFeelMicrophoneOverlay : MonoBehaviour
    {
        private const float ReferenceHeight = 1080.0F;
        private const float PanelWidth = 520.0F;
        private const float PanelHeight = 560.0F;
        private const float Margin = 32.0F;

        private IReadOnlyList<MicrophoneDevice> microphones = Array.Empty<MicrophoneDevice>();
        private bool loading;
        private string? message;
        private Vector2 scroll;

        private CursorLockMode previousLockState;
        private bool previousCursorVisible;

        public bool IsVisible { get; private set; }

        public void Show()
        {
            if (IsVisible)
                return;

            IsVisible = true;
            previousLockState = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Refresh();
        }

        public void Hide()
        {
            if (!IsVisible)
                return;

            IsVisible = false;
            Cursor.lockState = previousLockState;
            Cursor.visible = previousCursorVisible;
        }

        private async void Refresh()
        {
            loading = true;
            message = UFeelAPI.IsAvailable ? null : "UFeel is unavailable";

            try
            {
                microphones = await UFeelAPI.SpeechToText.GetMicrophonesAsync();
                if (UFeelAPI.IsAvailable && microphones.Count == 0)
                    message = "No microphone found";
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                message = exception.Message;
            }
            finally
            {
                loading = false;
            }
        }

        private async void Select(string? id, string label)
        {
            loading = true;
            message = null;

            try
            {
                if (!await UFeelAPI.SpeechToText.SelectMicrophoneAsync(id))
                {
                    message = $"\"{label}\" is no longer available";
                    Refresh();
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                message = exception.Message;
            }
            finally
            {
                loading = false;
            }
        }

        private void OnEnable() => UFeelAPI.SpeechToText.MicrophonesChanged += OnMicrophonesChanged;

        private void OnDisable()
        {
            UFeelAPI.SpeechToText.MicrophonesChanged -= OnMicrophonesChanged;
            Hide();
        }

        private void OnMicrophonesChanged()
        {
            if (IsVisible)
                Refresh();
        }

        private void OnGUI()
        {
            HandleKeys(Event.current);

            if (!IsVisible)
                return;

            float scale = Screen.height / ReferenceHeight;
            Matrix4x4 previousMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1.0F));

            float width = Screen.width / scale;
            Rect area = new(
                Mathf.Max(Margin, width - PanelWidth - Margin),
                (ReferenceHeight - PanelHeight) / 2,
                Mathf.Min(PanelWidth, width - (2 * Margin)),
                PanelHeight);

            GUILayout.BeginArea(area, GUI.skin.box);
            DrawPanel();
            GUILayout.EndArea();

            GUI.matrix = previousMatrix;
        }

        private void DrawPanel()
        {
            GUILayout.Label("Speech microphone");

            scroll = GUILayout.BeginScrollView(scroll);
            GUI.enabled = !loading && UFeelAPI.IsAvailable;
            DrawChoice(UFeelText.DefaultMicrophone(microphones), null);
            foreach (MicrophoneDevice microphone in microphones)
                DrawChoice(microphone.Name, microphone.Id);
            GUI.enabled = true;
            GUILayout.EndScrollView();

            if (loading)
                GUILayout.Label("Loading...");
            else if (message != null)
                GUILayout.Label(message);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh") && !loading)
                Refresh();
            if (GUILayout.Button($"Close ({UFeelAPI.SpeechToText.MicrophoneSelectorKey})"))
                Hide();
            GUILayout.EndHorizontal();
        }

        private void DrawChoice(string label, string? id)
        {
            bool selected = id == UFeelAPI.SpeechToText.SelectedMicrophoneId;
            if (GUILayout.Toggle(selected, label, GUI.skin.button) && !selected)
                Select(id, label);
        }

        private void HandleKeys(Event current)
        {
            if (current.type != EventType.KeyDown || UFeelAPI.EyeTracking.IsCalibrating)
                return;

            if (current.keyCode == UFeelAPI.SpeechToText.MicrophoneSelectorKey)
            {
                if (IsVisible)
                    Hide();
                else
                    Show();

                current.Use();
            }
            else if (current.keyCode == KeyCode.Escape && IsVisible)
            {
                Hide();
                current.Use();
            }
        }
    }
}
