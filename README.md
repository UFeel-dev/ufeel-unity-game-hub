# UFeel Game Hub

[![Discord](https://img.shields.io/discord/1461581735631523900.svg)](https://discord.gg/TxYTDXMJkM)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](./LICENSE)
![Unity](https://img.shields.io/badge/Unity-%23000000.svg?logo=unity&logoColor=white)
![Linux](https://img.shields.io/badge/Linux-FCC624?logo=linux&logoColor=black)

![C++](https://img.shields.io/badge/c++-%2300599C.svg?style=for-the-badge&logo=c%2B%2B&logoColor=white)
![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=csharp&logoColor=white)
![Espressif](https://img.shields.io/badge/espressif-E7352C.svg?style=for-the-badge&logo=espressif&logoColor=white)

A Unity project demonstrating the [UFeel](Packages/com.ufcorp.ufeel) input package - emotion detection, eye tracking, speech recognition, and heart rate sensing as Unity gameplay inputs.

You can find the website [here](https://ufeel.guitt.re).

## Documentation

| Document | Description |
| --- | --- |
| [Architecture](ARCHITECTURE.md) | System architecture and component overview |
| [API Reference](Packages/com.ufcorp.ufeel/Runtime/API.md) | UFeel Unity API usage guide |
| [Game Hub](Documentation/GameHub/README.md) | Game Hub overview and controls |
| [Scene Reference](Documentation/GameHub/Scenes.md) | Scene-by-scene breakdown |
| [Testing & Debugging](Documentation/GameHub/TestingTips.md) | Testing and debugging guide |
| [Changelog](CHANGELOG.md) | Release history |
| [Contributing](CONTRIBUTING.md) | Contribution guidelines |
| [Disclaimer](DISCLAIMER.md) | Limitations and prohibited use |

## Quick Start

Needs Linux x86_64, Unity 6000.0.58f2, Git LFS (native libraries and models), a webcam, and a microphone for the speech games.

1. Clone the [repository](https://github.com/UFeel-dev/ufeel-unity-game-hub) with Git LFS.
2. Open it in Unity Hub.
3. Open `Assets/Hub/Scenes/GameHub.unity` and press Play.

The UFeel package is embedded in `Packages/com.ufcorp.ufeel`. `tools/update-ufeel.sh` updates it from a ufeel build.

## License

Apache 2.0 License - see [LICENSE](LICENSE).
