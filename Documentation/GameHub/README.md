# Game Hub

> [<- Back to README](../../README.md)

Unity project showcasing the [UFeel](../../Packages/com.ufcorp.ufeel) biometric input package through a hub of small games.

![Game Hub](../Assets/game_hub.png)

## Features

- First-person hub with a door carousel leading to each game
- Room menu with one sub-menu per category: pick a room with the mouse, the arrow keys, its door number or the eyes
- Games driven by emotion detection, eye tracking, speech to text and heart rate
- Additive scene loading, one assembly per game
- Round room with textures, floating dust and a coloured light above each room door (`Assets/Hub/Prefabs/HubRoom.prefab`)
- The selected door stops halfway down and a staircase rises up to it, the room behind is shown through the doorway
- Carousel sounds in `Assets/Hub/Audio` (regenerate them with **Tools > Hub > Bake Sounds**), and a welcome screen on first launch

## Technologies

- Unity 6000.0.58f2 (URP)
- UFeel (emotion detection, eye tracking and speech to text through the webcam and microphone, heart rate through a sensor)
- C#, Unity Physics and the Input System

## Keyboard shortcuts

Defaults below. Every key except Esc, Enter, arrows, digits and Backspace can be rebound in **Pause > Settings > Controls**. Binding a key already in use swaps the two. Keys are physical positions (WASD on QWERTY is ZQSD on AZERTY).

| Key | Action |
| --- | --- |
| **WASD / ZQSD / Arrow keys** | Move around |
| **Mouse** | Look around |
| **E** | Enter the door in front of you |
| **N** | Toggle eye control of the room menu: look left or right to move (it repeats while you keep looking), close your eyes for a moment to select. The indicator under the menu shows both |
| **F** | Toggle faster carousel rotation |
| **T** | Toggle the flashlight (intensity is in the settings) |
| **Tab** | Toggle between looking around and using the menus |
| **0-9**, **Enter** | Go to the door with this number |
| **Esc** | Back in the room menu, otherwise toggle the pause menu (settings, controls, both calibrations, microphone) |
| **F1** | Camera feedback and debug data |
| **F7** | Emotion calibration |
| **F8** | Eye-tracking calibration |
| **F9** | Microphone selector |

## Documentation

| Document | Description |
| --- | --- |
| [Scene Reference](Scenes.md) | Scene-by-scene breakdown |
| [Testing & Debugging](TestingTips.md) | Testing and debugging guide |
| [API Reference](../../Packages/com.ufcorp.ufeel/Runtime/API.md) | UFeel Unity API |
