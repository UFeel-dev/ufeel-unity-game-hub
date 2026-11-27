# Setup Instructions

> [<- Game Hub](README.md)

## Requirements

- Linux x86_64
- Unity 6000.0.58f2
- Git LFS (native libraries and models)
- A webcam, and a microphone for the speech games

## Steps

1. Clone the repository with Git LFS enabled.
2. Open the project in Unity Hub.
3. Open `Assets/Hub/Scenes/GameHub.unity` and press Play.

## Notes

- The UFeel package is embedded in `Packages/com.ufcorp.ufeel`, no import is needed. `tools/update-ufeel.sh` updates it from a ufeel build.
- Webcam access is required. Permissions may be requested on first launch.

**See also:** [Scene Reference](Scenes.md) - [Testing & Debugging](TestingTips.md)
