# Scene Reference

> [<- Game Hub](README.md)

Scenes live in `Assets/Hub/Scenes` and `Assets/Games/<Category>/<Game>/Scenes`. Every scene below is in the build settings.

## Game Hub (`GameHub`)

**Role:** Lobby used to reach every game.

- First-person controller with mouse look.
- Door carousel: pick a room in the room menu (one sub-menu per category, with the mouse, the arrow keys, the door number or the eyes). The door comes down halfway and a staircase rises to it. Climb it and press **E**. Picking another door lowers the staircase first.
- Doors whose game is not ready show "In progress" and stay closed.

**Main scripts:** `CarouselRotator`, `DoorSelectionManager`, `DoorPicker`, `DoorTrigger`, `SceneLoader`

## Emotion-Based Games

### Maze

**Role:** Emotion-driven labyrinth.

**Main script:** `MazeManager`

**See also:** [Maze README](../Emotions/Maze/README.md)

### Animals

**Role:** Emotion-based memory game.

**Main script:** `AnimalsManager`

**See also:** [Animals README](../Emotions/Animals/README.md)

## Eye-Tracking Games

### Car

**Role:** Vehicle driven with the eyes.

**Main script:** `VehicleController`

**See also:** [Car README](../EyeTracking/Car/README.md)

### Survivor

**Role:** Eye-tracking survival game.

**Status:** In progress, its door shows "In progress".

**Main script:** `SurvivorGameManager`

## Speech-To-Text Games

### Escape Room

**Role:** Voice-controlled escape room (French commands).

**Main script:** `SpeechManager`

**See also:** [Escape Room README](../SpeechToText/EscapeRoom/README.md)

## Heart Rate Games

### Rhythm Game

**Role:** Four-lane rhythm game whose difficulty follows the heart rate.

**Main script:** `RhythmGameManager`

**See also:** [Rhythm Game README](../HeartRate/RhythmGame/README.md)

### Horror Room

**Role:** Proof of concept: a heartbeat sound and a red flash follow the heart rate.

**Status:** In progress, its door shows "In progress".

**Main script:** `HeartbeatEffects`
