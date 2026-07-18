# Plan: Make Rope Originate from Monke Arm

## Goal
Update the rope system so that the rope visually and physically originates from the tip of the `MonkeArm` sprite instead of the player's center.

## Files to Modify
1. `player/player.cs`
2. `rope/Rope.cs`

## Steps

### 1. Update `player/player.cs`
- Add a public method `GetArmTipPosition()` that calculates the global position of the tip of the `MonkeArm` sprite.
  - The tip position can be calculated by taking the arm's global position and adding an offset in the direction of the arm's rotation.
  - Since the arm is rotated, we can use `monkeArm.GlobalPosition + new Vector2(Mathf.Cos(monkeArm.Rotation), Mathf.Sin(monkeArm.Rotation)) * armLength`.
  - We need to determine the `armLength` based on the sprite's scale and texture size. For simplicity, we can expose a configurable offset or calculate it from the sprite's `Scale` and `Texture` size.
- Update the `rope` reference to use this new position when initializing or updating the rope's start point.

### 2. Update `rope/Rope.cs`
- Modify `StartShot()` to use the arm tip position as the `shotOrigin` instead of `player.GlobalPosition`.
- Modify `UpdateAim()` to use the arm tip position as the raycast origin.
- Modify `InitializeRope()` and `UpdateVerlet()` to use the arm tip position as the end point of the rope (segment `MaxSegments`) instead of `player.GlobalPosition`.
- Add a reference to the `player` node to access the new `GetArmTipPosition()` method.

## Implementation Details
- The `MonkeArm` sprite has a scale of `0.3`. We need to estimate the length of the arm in pixels to calculate the tip position accurately.
- If the arm texture is, say, 100 pixels long, the scaled length is 30 pixels. The tip would be 30 pixels away from the center in the direction of rotation.
- We will add a property `float armTipOffset` to `player.cs` to allow tuning this distance.
- The rope's visual line (`RopeLine`) and physics simulation will now start/end at this new point.
