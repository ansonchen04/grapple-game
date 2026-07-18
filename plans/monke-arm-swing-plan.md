# Plan: Add Monke Arm Sprite During Swing Mode

## Goal
Display `monke_arm.png` centered on the player when the player is in "swing_body" mode (airborne or grappled).

## Files to Modify
1. `player/player.tscn`
2. `player/player.cs`

## Steps

### 1. Update `player/player.tscn`
- Add a new `Sprite2D` node named `MonkeArm`.
- Set its `texture` to `res://sprites/player/monke_arm.png`.
- Set its `position` to `Vector2(0, 0)` to center it on the player.
- Set its `visible` property to `false` initially (since it should only show during swing mode).

### 2. Update `player/player.cs`
- Add a private field `Sprite2D monkeArm`.
- In `_Ready()`:
  - Get the `MonkeArm` node using `GetNode<Sprite2D>("MonkeArm")`.
- In `_PhysicsProcess()`:
  - Update the visibility logic alongside the existing sprite switching logic.
  - If `isSwinging` or `!IsOnFloor()`, set `monkeArm.Visible = true`.
  - Otherwise, set `monkeArm.Visible = false`.

## Implementation Details
- The `monke_arm.png` file is located at `res://sprites/player/monke_arm.png`.
- The arm should be centered on the player's origin.
- The visibility should toggle every frame in `_PhysicsProcess` to match the state changes.
