# Plan: Switch Player Sprite Based on State

## Goal
Switch the player's sprite from `standing.png` to `swing_body.png` when the player is airborne or grappled to a rope.

## Files to Modify
1. `player/player.tscn`
2. `player/player.cs`

## Steps

### 1. Update `player/player.tscn`
- Add a new `ext_resource` for the `swing_body.png` texture.
- No changes to the `Sprite2D` node itself are needed in the scene file, as we will handle the texture switching in code.

### 2. Update `player/player.cs`
- Add a private field `Texture2D swingTexture` to hold the reference to `swing_body.png`.
- In `_Ready()`:
  - Load the `swing_body.png` texture into `swingTexture`.
  - Store the current `standing.png` texture in a new private field `Texture2D standingTexture` (or just use the existing `playerSprite.Texture` as the reference).
- In `_PhysicsProcess()`:
  - Determine the player's state:
    - `isSwinging`: Check if `rope.ropeState` is `RopeState.Hooked` or `RopeState.Retracting`.
    - `isAirborne`: Check if `!IsOnFloor()`.
  - If `isSwinging` or `isAirborne`, set `playerSprite.Texture = swingTexture`.
  - Otherwise, set `playerSprite.Texture = standingTexture`.
  - Ensure this logic runs after the sprite flip logic to avoid overwriting the flip state (though `Texture` and `FlipH` are independent properties).

## Implementation Details
- The `swing_body.png` file is located at `res://sprites/player/swing_body.png`.
- The `standing.png` file is located at `res://sprites/player/standing.png`.
- The sprite switching should happen every frame in `_PhysicsProcess` to ensure immediate visual feedback when the state changes.
