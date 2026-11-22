# Rhythm Game

> [<- Scene Reference](../../GameHub/Scenes.md)

![](../../Assets/rhythm_game.png)

## Role

Heart-rate-adaptive rhythm game.

## Gameplay

- Directional prompts appear on screen.
- The player must press the corresponding inputs at the correct time.
- The game's speed and music follow the player's heart rate (60 to 180 BPM, 60 BPM without a reading).
- Increased heart rate results in a more challenging experience.
- Missing notes drains the health bar, the game ends when it is empty.

## Objective

Maintain accuracy while adapting to the changing rhythm of the game.

## Main Script

`RhythmGameManager`

## Keyboard Shortcuts

| Key | Action |
|---|---|
| **Any key** | Start the game |
| **Arrow keys** | Press the matching note |
| **A** (**Q** on AZERTY) | Pan the camera to the left |
| **D** | Pan the camera to the right |
