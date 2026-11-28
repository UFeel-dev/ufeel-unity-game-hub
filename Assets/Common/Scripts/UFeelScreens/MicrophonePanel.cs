using System;
using System.Collections.Generic;
using UFeel;
using UnityEngine;

namespace GameHub.Common
{
    public sealed class MicrophonePanel
    {
        private const int Rows = 6;
        private const float ScrimOpacity = 0.86f;
        private const int RefreshAction = 1;
        private const int CloseAction = 2;
        private const string InUse = "In use";

        private static readonly KeyHint[] Hints =
        {
            new("Select", 0, KeyHint.Up, KeyHint.Down),
            new("Use", 0, "Enter"),
            new("Refresh", RefreshAction, "R"),
            new("Close", CloseAction, "Esc"),
        };

        private readonly Scrim scrim;
        private readonly ListScreen screen;
        private readonly KeyBar keys;
        private readonly List<MenuEntry> entries = new();
        private readonly List<string?> ids = new();

        private IReadOnlyList<MicrophoneDevice> microphones = Array.Empty<MicrophoneDevice>();
        private bool loading;
        private string? message;
        private CursorLockMode previousLockState;
        private bool previousCursorVisible;

        public MicrophonePanel(UiKit kit, Transform parent)
        {
            scrim = new Scrim(kit, parent, ScrimOpacity);
            screen = new ListScreen(kit, parent, "Microphone", Rows);
            keys = new KeyBar(kit, parent);
            keys.Set(Hints);
            keys.SetVisible(false);
        }

        private bool IsVisible { get; set; }

        public void Toggle()
        {
            if (IsVisible)
                Hide();
            else
                Show();
        }

        public void Show()
        {
            if (IsVisible)
                return;

            IsVisible = true;
            UiFocus.Capture(this);
            previousLockState = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
            PointerInput.ShowCursor(true);
            scrim.Show(true, Fader.DefaultSeconds);
            keys.SetVisible(true);
            Render(true);
            Refresh();
        }

        public void Hide()
        {
            if (!IsVisible)
                return;

            IsVisible = false;
            UiFocus.Release(this);
            Cursor.lockState = previousLockState;
            Cursor.visible = previousCursorVisible;
            scrim.Show(false, Fader.DefaultSeconds);
            screen.Hide();
            keys.SetVisible(false);
        }

        public void OnMicrophonesChanged()
        {
            if (IsVisible)
                Refresh();
        }

        public void Tick(float deltaTime, PointerInput mouse)
        {
            scrim.Tick(deltaTime);
            screen.Tick(deltaTime);
            if (!IsVisible || !UiFocus.Has(this))
                return;

            int clicked = keys.Clicked(mouse);
            if (clicked == CloseAction)
            {
                GameSounds.Play(SoundCue.MenuBack);
                Hide();
                return;
            }

            if (clicked == RefreshAction || LayoutInput.GetKeyDown(KeyCode.R))
            {
                GameSounds.Play(SoundCue.MenuConfirm);
                Refresh();
                return;
            }

            switch (screen.List.HandleInput(deltaTime, mouse))
            {
                case MenuAction.Back:
                    Hide();
                    break;
                case MenuAction.Confirmed when !loading && UFeelAPI.IsAvailable:
                    Select(ids[screen.List.Selected], entries[screen.List.Selected].Label);
                    break;
            }
        }

        private void Render(bool resetSelection)
        {
            if (!IsVisible)
                return;

            string? selectedId = UFeelAPI.SpeechToText.SelectedMicrophoneId;
            entries.Clear();
            ids.Clear();
            AddEntry(UFeelText.DefaultMicrophone(microphones), null, selectedId);
            foreach (MicrophoneDevice microphone in microphones)
                AddEntry(microphone.Name, microphone.Id, selectedId);

            int selected = resetSelection ? Math.Max(0, ids.IndexOf(selectedId)) : Math.Min(screen.List.Selected, entries.Count - 1);
            screen.Show("Microphone", entries, selected);
            screen.SetFooter(loading ? "Loading..." : message ?? string.Empty);
        }

        private void AddEntry(string label, string? id, string? selectedId)
        {
            entries.Add(new MenuEntry(label, id == selectedId ? InUse : string.Empty));
            ids.Add(id);
        }

        private async void Refresh(string? notice = null)
        {
            loading = true;
            message = UFeelAPI.IsAvailable ? notice : "UFeel is unavailable";
            Render(false);

            try
            {
                microphones = await UFeelAPI.SpeechToText.GetMicrophonesAsync();
                if (UFeelAPI.IsAvailable && microphones.Count == 0)
                    message = "No microphone found";
            }
            catch (OperationCanceledException)
            {
            }
            catch (InvalidOperationException exception)
            {
                message = exception.Message;
                Debug.LogException(exception);
            }
            finally
            {
                loading = false;
            }

            Render(true);
        }

        private async void Select(string? id, string label)
        {
            loading = true;
            message = null;
            Render(false);
            bool selected = true;

            try
            {
                selected = await UFeelAPI.SpeechToText.SelectMicrophoneAsync(id);
            }
            catch (OperationCanceledException)
            {
            }
            catch (InvalidOperationException exception)
            {
                message = exception.Message;
                Debug.LogException(exception);
            }
            finally
            {
                loading = false;
            }

            if (!selected)
                Refresh($"\"{label}\" is no longer available");
            else
                Render(false);
        }
    }
}
