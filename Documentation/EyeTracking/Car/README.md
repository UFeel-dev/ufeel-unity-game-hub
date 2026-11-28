# Car

> [<- Scene Reference](../../GameHub/Scenes.md)

![Car](../../Assets/car.png)

## Role

Vehicle navigation using eye tracking.

## Gameplay

- Walk into the vehicle to get in.
- Look where you want to go: left or right of the centre steers, higher drives faster, around the centre brakes, lower reverses. A ring shows where you look.
- **G** switches to the direction mode: look up or down for a moment to shift between **Drive**, **Stop** and **Reverse**, left or right to steer.
- The indicator at the bottom right shows the gear and the steering.
- Crossing the finish line returns to the hub.

## Objective

Drive the vehicle to the finish line using eye tracking.

## Main Script

`VehicleController`

## Keyboard shortcuts

| Key | Action |
| --- | --- |
| **W** (**Z** on AZERTY) / **S** | Accelerate / reverse while held, fallback for the gears |
| **A** (**Q** on AZERTY) / **D** | Steer, fallback for the eyes |
| **G** | Switch between screen position and direction driving |

Keys can be rebound in **Pause > Settings > Controls**.
