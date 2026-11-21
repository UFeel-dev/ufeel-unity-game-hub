# UFeelAPI – Unity API Reference

> [<- Back to README](../../../README.md)

`UFeelAPI` is created before the first scene and lives across scene loads. Native processing runs on a background thread. Every getter below reads the latest results without blocking.

A feature runs only while it is read (stops ~2 s after the last read). Speech runs between `StartDetection` and `StopDetection`.

## Emotion

```csharp
EmotionType? dominant = UFeelAPI.Emotion.Dominant; // null without a face
EmotionResult current = UFeelAPI.Emotion.Current;  // Status, Scores ("happy" = 0.82, ...)
```

## Eye tracking

```csharp
EyeTrackingDirection directions = UFeelAPI.EyeTracking.Directions;
bool left = directions.HasDirection(EyeTrackingDirection.Left);
Vector2 gaze = UFeelAPI.EyeTracking.Gaze;         // -1..1, 1 = right/bottom screen edge
UFeelAPI.EyeTracking.HeadWeight = 1f;             // 0 = eyes only (default), 1 = eyes + head
bool closed = UFeelAPI.EyeTracking.IsBlinking;    // both eyes closed
uint blinks = UFeelAPI.EyeTracking.BlinkCount;    // +1 per blink, compare with the previous value

Vector2 position = UFeelAPI.EyeTracking.Position; // 0..1, origin top-left
```

Blinking and gaze direction work without calibration. Calibration refines the gaze range. The screen position needs a calibration. The saved profile (`~/.local/share/UFeel/eyeTracking-profile-v3.txt`, shared with the ufeel test harness) loads at startup. Otherwise press **F8** (`UFeelAPI.EyeTracking.CalibrationKey`) or call `UFeelAPI.EyeTracking.Calibrate()`. **Escape** cancels.

## Speech to text (French model)

```csharp
UFeelAPI.SpeechToText.StartDetection();
string? text = UFeelAPI.SpeechToText.Current; // null while stopped
UFeelAPI.SpeechToText.StopDetection();
```

The system default microphone is used until another one is selected. **F9** (`UFeelAPI.SpeechToText.MicrophoneSelectorKey`) opens the microphone selector, which is also shown in the pause menu. From code:

```csharp
IReadOnlyList<MicrophoneDevice> microphones = await UFeelAPI.SpeechToText.GetMicrophonesAsync();
await UFeelAPI.SpeechToText.SelectMicrophoneAsync(microphones[0].Id); // null: default microphone
string? selected = UFeelAPI.SpeechToText.SelectedMicrophoneId; // null: default microphone
```

The choice is saved in `PlayerPrefs` and restored at startup. Listing restarts a running detection to rescan the devices. `UFeelAPI.SpeechToText.MicrophonesChanged` is raised when a microphone is plugged, unplugged or becomes the system default, and the selector refreshes itself. A selected microphone that disappears falls back to the default one until it comes back.

## Heart rate

```csharp
uint bpm = UFeelAPI.HeartRate.Current; // 0 without a valid reading
```

## Rules

Each facade can register condition to action rules, evaluated once per frame on the main thread:

```csharp
RuleKey key = UFeelAPI.Emotion.TriggerActionOnEmotionContinuous(EmotionType.Happy, OnHappy);
UFeelAPI.SpeechToText.TriggerActionOnSpeechOnce("ouvre", OpenDoor); // case-insensitive "contains"
UFeelAPI.HeartRate.TriggerActionOnHeartRateOnce(100, Scare, tolerance: 5);
UFeelAPI.EyeTracking.TriggerActionOnDirectionOnce(EyeTrackingDirection.Left, TurnLeft);
UFeelAPI.RemoveRule(key);
```

`*Once` rules are removed after firing. `*Continuous` rules fire every frame while the condition holds.

## Updating the native library

```bash
tools/update-ufeel.sh ../ufeel/package/release # package built with ./build.sh all --adapter csharp
```

This syncs `libufeel_wrapper.so`, `dependencies/`, `models/` and the C# adapter (`Runtime/Core`). Linux player builds get `dependencies/` and `models/` copied automatically.

**See also:** [Architecture](../../../ARCHITECTURE.md) - [Testing & Debugging](../../../Documentation/TestingRoom/TestingTips.md)
