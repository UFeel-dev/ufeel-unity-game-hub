# Changelog

> [<- Back to README](README.md)

## [2026-11]

### Changed

- UFeel library updated to the latest release, used through the shared `UFeel.Core` C# adapter (`Runtime/Core`).
- Native processing runs on a background thread. Features run only while read.
- `UFeelAPI.Gaze`/`GazeDirection` renamed to `UFeelAPI.EyeTracking`/`EyeTrackingDirection`.
- `EyeTracking.Directions` and `IsBlinking` work without calibration. `IsBlinking` means both eyes closed, with hysteresis. Blinks no longer interrupt the position or directions.
- Project reorganized by feature: `Assets/Hub`, `Assets/Common`, `Assets/Games/<Category>/<Game>` and `Assets/ThirdParty`, with one `GameHub.*` assembly and namespace per feature.
- The Testing Room is now the Game Hub (`GameHub` scene, `Documentation/GameHub`).
- Game scripts use private serialized fields, shared input, scene flow and heart rate helpers, and no longer compile into the UFeel package assembly.
- Rhythm game rebuilt in the style of osu!mania. The heart rate sets the difficulty level instead of the tempo.
- The rhythm game's UI kit moved to `GameHub.Common` (`Common/Scripts/UI/Kit`). The hub, the pause menu, the Animals, Maze and Escape Room HUDs and the door labels use it, with the Barlow Semi Condensed font.
- Door selection is a room menu with one sub-menu per category: mouse, arrow keys, typed door number or eyes. The hub shows its key shortcuts.
- While the mouse is free (Tab), the player stands still and the arrow keys drive the menus.
- Eye control of the room menu: looking left or right moves after a short dwell and repeats while held, closing the eyes for 0.7 s selects, with an indicator under the menu. It replaces the double blink.
- The car is now fully driven with the eyes: looking up or down shifts between Drive, Stop and Reverse, looking left or right steers, and a gear indicator shows both. The keyboard stays as a fallback.
- The pause menu offers Lobby only outside the hub.
- The microphone selector opens from the pause menu instead of next to it.
- The Maze is now a generated labyrinth with seven levels, walkable fog doors instead of teleports, and new room visuals.

### Added

- Eye-tracking calibration overlay (F8) and automatic loading of the shared calibration profile.
- Linux player builds ship `dependencies/` and `models/`.
- `tools/update-ufeel.sh` to sync a ufeel package.
- `EyeTracking.Gaze`, `BlinkCount` and `HeadWeight`.
- `EyeTracking.IsInUse`, `EyeTracking.CancelCalibration()` and `UFeelAPI.CameraFeed`.
- Rhythm game song select, Base and Scaling difficulty sliders, options and a results screen with a heart rate graph.
- Rhythm game charts generated from the song audio, with hold notes.
- Rhythm game options: arrow or circle notes, key labels, and song speed from 50% to 200% following the heart rate (100% by default).
- **Tools > Rhythm Game > Add Song** to add rhythm game songs.
- Maze level select with saved progress and an Unlimited mode.
- Maze levels defined as assets (`MazeLevelDefinition`, `MazeCatalog`).
- Maze special rooms (hint, bonus, trap), one-way and closing doors, room shapes and a minimap.
- `MenuEntry` icon and disabled state, `UiCanvas.Create` option to stay visible under menus.

### Removed

- Empty Environment, Runner and UnlockWorld scenes, the API testing scene, third-party demo scenes and unused assets.
- Unused packages (HDRP, Visual Scripting, Timeline, AI Navigation, Collab, VS Code, Multiplayer Center, Test Framework).
- The inactive mirror workflow and the `tools/refresh-package` script.
- Rhythm game lane-changing notes, camera panning and the `Activator` and `PerturbatorDetector` tags.
- UFeel IMGUI overlays, `EyeTracking.CalibrationKey`, `EyeTracking.Calibrate()`, `SpeechToText.MicrophoneSelectorKey`, `IsMicrophoneSelectorVisible` and `Show/HideMicrophoneSelector`.

### Fixed

- Speech rules matched in the wrong direction and fired on empty speech.
- Calibration profile paths were passed as UTF-16.
- The Heart Rate - Horror door targeted a missing scene.
- Picking another door before the carousel stopped left a door lowered, and picking a door twice kept it raised.
- The pause menu Quit and hub door selection buttons targeted wrong assemblies.
- The Escape Room camera and Horror Room red filter references were lost.
- The rhythm game stalled and the Horror Room effects stopped without a heart rate reading.
- The pause state persisted across play sessions in the editor.

## [2026-07]

### Added

- Adding Levenshtein distance in Escape room scene to improve speech recognition tolerance.
- Adding progress bar in Animals's memory game.
- Adding back Eye Tracking Car scene
- Integrating our own emotion detection model


## [2026-06]

### Added

- Animals's memory game.

### Fixed

- Escape room can now be access through Hub (fix name issue).

## [2026-05]

### Added

- Debug HUD toggle (`UseDefaultDebugHUD`) and Escape Room-specific HUD customization.

### Fixed

- Speech manager now starts the API reliably, general cleanup.

## [2026-04]

### Added

- Static analysis baseline via `.editorconfig` and `Directory.Build.props`.
- Heartbeat rhythm game content for Testing Room (scenes/assets/models) and in-room guidance elements.

### Fixed

- Removed unused Heartbeat room assets and tuned gameplay values.

## [2026-03]

### Added

- Heart rate Horror Room POC (scene, effects scripts, audio assets).
- Escape Room scene.

## [2026-02]

### Added

- Reworked carousel system with a cleaned-up file/scene structure.
- Debug stack: `UFeelDebugHUD` and visual debug mode for API/receivers.
- Project documentation: API README, internal documentation links, and `ARCHITECTURE.md`.
- Expanded `DISCLAIMER.md` with additional prohibited use cases and non-responsibility clauses.

### Changed

- Refactored `UFeelAPI` to static usage.
- Updated trigger behavior and Python server integration.
- Improved door selection UI sizing.

### Fixed

- Naming consistency for UFeel and asmdef/asmref files.
- Unique file handling per data type.
- Removed outdated code and corrected graphics settings details.
- Fixed carousel door behavior and Testing Room door setup.

### Merged

- Merged `api-unity` into Testing Room.
- Merged Testing Room into `main`.

## [2026-01]

### Added

- Apache 2.0 license.
- `CONTRIBUTING.md`.
- `DISCLAIMER.md`.
- Documentation move/restructure for Testing Room.

## [2025-12]

### Added

- Speech-to-text detection integrated into API (Python + Unity receiver).
- Heart rate sensor (Python) and Unity API receiver.
- Frame text rendering for debugging.

## [2025-10 – 2025-11]

### Added

- Doors and Testing Room scenes for speech recognition and heartbeat.
- Base version without detection.

### Changed

- Upgraded Unity to stable LTS version and restored flashlight support.

### Fixed

- Emotion detection and eye tracking behavior in Unity scenes.

## [2025-07]

### Changed

- Model activation logic improvements and notes on multi-model frame handling.

## [2025-06]

### Added

- Base API and initial API documentation.
- TCP transport and `ClientBase` foundation.
- Enable/disable Python detectors from C#.
- Integrated emotion detection into Testing Room.
- Eye tracking template improvements (sphere movement, distance handling, vehicle control, finish line).
- Textured terrain for eye tracking scene.

### Changed

- Door behavior improvements (lowering/rotation) and scene-loading synchronization before door opening.
- Disable room after entering a new scene.

### Fixed

- Asynchronous Python process handling.

## [2025-05]

### Added

- Testing Room base lighting and scene improvements.
- Carousel system, door prefabs, automated door placement tooling, and door selection panel.
- Chain extension system, scene swapping, and door open-before-swap flow.
- Base corridor with additive scene loading, additive load on door trigger.

### Changed

- Directory restructuring and door setup workflow improvements.
- Larger door trigger and scene alignment adjustments.
- Front-wall split/open logic for transitions, door positioning against walls.

## [2025-03]

### Added

- Initial Unity project setup and configuration.
- Emotion detection and eye tracking scenes, servers, and Unity integration.
- Generic receiver/server-controller layer and singleton-based management.
- Demo game with emotion-driven door logic, scoring, and debug controls.

### Changed

- Dependency/requirements updates and demo scene refinements.

### Fixed

- Disabled/adjusted mirroring GitHub Actions and improved eye-tracker stability.
