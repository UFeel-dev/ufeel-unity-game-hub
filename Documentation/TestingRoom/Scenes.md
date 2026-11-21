# Scene Reference

> [<- Testing Room](README.md)

## Keyboard shortcuts

These keyboard shortcuts are available in all scenes:

| Key | Action |
| --- | --- |
| **ZQSD / Arrow keys** | Move around |
| **M** | Toggle Pause Menu |
| **V** | Start again sensors |

## Base Scene (TestingRoom)

### Role

Main hub used to access all available experiences.

### Features

- First-person controller.
- Mouse controls camera rotation.
- Scene selection through interactive doors.
- Lighting is intentionally disabled.

All other scenes can be accessed by entering the corresponding door.

## Emotion-Based Experiences

### Maze

**Role:** Emotion-driven labyrinth experience.

**Main Script:** `MazeManager`

**See also:** [Maze README](../Emotions/Maze/README.md)

### Animals

**Role:** Emotion-based memory game.

**Main Script:** `AnimalsManager`

**See also:** [Animals README](../Emotions/Animals/README.md)

## Eye Tracking Experiences

### Car

**Role:** Vehicle control using eye tracking.

**Main Script:** `VehicleController`

**See also:** [Eye Tracking README](../EyeTracking/Car/README.md)

### Survivor

**Role:** Eye-tracking survival experience.

**Status:** Planned for future development.

## Speech-To-Text Experiences

### EscapeRoom

**Role:** Voice-controlled escape room.

**Main Script:** `SpeechManager`

**See also:** [EscapeRoom README](../SpeechToText/EscapeRoom/README.md)

## Heart Beat Experiences

### RhythmGame

**Role:** Heart-rate-adaptive rhythm game.

**Main Script:** `GameManager`

**See also:** [RhythmGame README](../HeartBeat/RhythmGame/README.md)
