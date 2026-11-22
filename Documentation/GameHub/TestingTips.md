# Testing & Debugging

> [<- Testing Room](README.md)

Instructions for testing and debugging the UFeel package across the main Testing Room scenes.

## General Requirements

- A working webcam with permissions granted (may be requested on first launch).
- No other application monopolizing the webcam (e.g., Zoom, Teams).

## Emotion Detection Scene

**Scene Purpose:** Demonstrates UFeel's facial emotion recognition by gating player progression through doors.

### Testing Steps for Emotion Detection

- From the base scene, enter the **Maze** scene via the carousel.
- The camera feed should activate automatically.
- Try displaying emotions like **happiness**, **surprise**, or **anger** as required by fog door labels.
- The path will open only when the correct emotion is held for a sufficient time.

### Troubleshooting Emotion Detection

- **Emotion not recognized?**
  - Check for sufficient front-facing light.
  - Ensure your face is fully visible in the webcam's field of view.

## Eye Tracking Scene

**Scene Purpose:** Controls a vehicle using gaze-based direction.

### Testing Steps for Eye Tracking

- From the base scene, enter the **Car** scene via the carousel.
- Ensure the eye-tracking logic works (gaze affects vehicle direction).

### Troubleshooting Eye Tracking

- **Eye tracking not precise enough?**
  - Check for sufficient front-facing light.
  - Ensure your face is fully visible in the webcam's field of view.

## Base Scene (TestingRoom)

**Scene Purpose:** Main menu hub used to load other scenes additively.

### Testing Steps for Testing Room

- Run `TestingRoom.unity`.
- Use the carousel to enter **EyeTracking** and **Emotion** scenes.
- Return to base between tests to ensure transitions preserve webcam availability.

### Troubleshooting Testing Room

- **Scene does not load**
  - Ensure all the scenes are referenced properly in the build settings.

## Tips

### Recommended Testing Practices

- Always test with a neutral face between expressions to avoid emotion confusion.
- Vary your facial expression intensity to check threshold tuning.
- Test with multiple people to evaluate detection robustness.

For integration help or feature requests, open an issue on the repository.

**See also:** [Scene Reference](Scenes.md) - [Setup Instructions](SetupInstructions.md) - [API Reference](../../Packages/com.ufcorp.ufeel/Runtime/API.md)
