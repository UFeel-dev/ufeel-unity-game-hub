# Testing & Debugging

> [<- Game Hub](README.md)

Instructions for testing and debugging the UFeel package across the Game Hub scenes.

## General Requirements

- A working webcam with permissions granted (may be requested on first launch).
- No other application monopolizing the webcam (e.g., Zoom, Teams).

## Emotion Detection Scene

**Scene Purpose:** Demonstrates UFeel's facial emotion recognition by gating player progression through doors.

### Testing Steps for Emotion Detection

- From the hub, enter the **Maze** door.
- The camera feed should activate automatically.
- Display the emotion written on a fog door (**happiness**, **surprise** or **fear**) and walk into it.
- The path opens once the emotion is held for about two seconds.

### Troubleshooting Emotion Detection

- **Emotion not recognized?**
  - Check for sufficient front-facing light.
  - Ensure your face is fully visible in the webcam's field of view.

## Eye Tracking Scene

**Scene Purpose:** Controls a vehicle using gaze-based direction.

### Testing Steps for Eye Tracking

- From the hub, enter the **Car** door and walk into the vehicle.
- Accelerate with **W** (**Z** on AZERTY) and check that looking left or right steers the vehicle.
- Press **F8** to calibrate if the gaze range feels too narrow.

### Troubleshooting Eye Tracking

- **Eye tracking not precise enough?**
  - Check for sufficient front-facing light.
  - Ensure your face is fully visible in the webcam's field of view.

## Game Hub (`GameHub`)

**Scene Purpose:** Main menu hub used to load other scenes additively.

### Testing Steps for the Game Hub

- Open `Assets/Hub/Scenes/GameHub.unity` and press Play.
- Use the carousel to enter each game, then come back through the pause menu (**M**, then the lobby button).
- Return to the hub between tests to check that transitions keep the webcam available.

### Troubleshooting the Game Hub

- **A door shows "In Progress"**
  - Its game is flagged as in progress, or its scene is missing from the build settings.

## Tips

### Recommended Testing Practices

- Always test with a neutral face between expressions to avoid emotion confusion.
- Vary your facial expression intensity to check threshold tuning.
- Test with multiple people to evaluate detection robustness.

For integration help or feature requests, open an issue on the repository.

**See also:** [Scene Reference](Scenes.md) - [Setup Instructions](SetupInstructions.md) - [API Reference](../../Packages/com.ufcorp.ufeel/Runtime/API.md)
