# Testing & Debugging

> [<- Game Hub](README.md)

## General Requirements

- A working webcam with permissions granted (may be requested on first launch).
- No other application monopolizing the webcam (e.g., Zoom, Teams).

## Emotion Detection Scene

**Scene Purpose:** Demonstrates UFeel's facial emotion recognition by gating player progression through doors.

### Testing Steps for Emotion Detection

- From the hub, enter the **Maze** door and pick **Level 1**.
- The camera feed should activate automatically.
- Walk towards a fog door: the closest one is outlined. Show the emotion written on it (**Happy** or **Surprised** in Level 1).
- After about two seconds the fog clears and you can walk through.

### Troubleshooting Emotion Detection

- **Emotion not recognized?**
  - Check for sufficient front-facing light.
  - Ensure your face is fully visible in the webcam's field of view.
  - Press **F7** to calibrate the emotions to your face: act each emotion shown on screen for a few seconds (about one minute in total). The profile is saved and reloaded at the next launch. **Settings > Reset emotion calibration** returns to the general model.

## Eye Tracking Scene

**Scene Purpose:** Drives a vehicle with the eyes.

### Testing Steps for Eye Tracking

- From the hub, enter the **Car** door and walk into the vehicle.
- Look higher on the screen to drive, left or right to steer, lower to reverse. The ring follows the gaze.
- Press **G** to switch to the direction mode, and **F8** to calibrate eye tracking if the gaze range feels too narrow.

### Troubleshooting Eye Tracking

- **Eye tracking not precise enough?**
  - Check for sufficient front-facing light.
  - Ensure your face is fully visible in the webcam's field of view.

## Game Hub (`GameHub`)

**Scene Purpose:** Main menu hub used to load other scenes additively.

### Testing Steps for the Game Hub

- Open `Assets/Hub/Scenes/GameHub.unity` and press Play.
- Use the carousel to enter each game, then come back through the pause menu (**Esc**, then the lobby button).
- Return to the hub between tests to check that transitions keep the webcam available.

### Troubleshooting the Game Hub

- **A door shows "In progress"**
  - Its game is flagged as in progress, or its scene is missing from the build settings.

## Tips

### Recommended Testing Practices

- Always test with a neutral face between expressions to avoid emotion confusion.
- Vary your facial expression intensity to check threshold tuning.
- Test with multiple people to evaluate detection robustness.

**See also:** [Scene Reference](Scenes.md) - [API Reference](../../Packages/com.ufcorp.ufeel/Runtime/API.md)
