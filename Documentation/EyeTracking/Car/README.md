# Car

> [<- Scene Reference](../../GameHub/Scenes.md)

![Car](../../Assets/car.png)

## Role

Vehicle navigation using eye tracking.

## Gameplay

- Walk into the vehicle to get in, press **E** to get out.
- Look up or down for a moment to shift between **Drive**, **Stop** and **Reverse**. Keep looking to shift again. **Stop** brakes.
- Look left or right to steer.
- The gear indicator at the bottom right shows the gear, the shift in progress and the steering.
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
| **E** | Exit the vehicle |
