# Plan: Always Use Swing Body Model

## Goal
Make the player always use the `swing_body` texture instead of switching between `standing` and `swing_body` based on state.

## Files to Modify
1. `player/player.cs`

## Steps

### 1. Update `player/player.cs`
- In `_Ready()`:
  - Remove the `standingTexture` field or stop using it.
  - Set `playerSprite.Texture` to `swingTexture` by default.
- In `_PhysicsProcess()`:
  - Remove the sprite switching logic (Step 8) that toggles between `standingTexture` and `swingTexture`.
  - The player will now always use the `swing_body` texture regardless of state.

## Implementation Details
- The `swingTexture` is already loaded in `_Ready()`.
- We can simply remove the conditional logic that switches textures and always use `swingTexture`.
- This simplifies the code and ensures consistent visual appearance.
