# Plan: Monke Arm Aiming and Rope Following

## Goal
Make the `MonkeArm` sprite follow the mouse cursor when aiming (rope hidden) and follow the rope/hook when the rope is shot or hooked.

## Files to Modify
1. `player/player.cs`

## Steps

### 1. Update `player/player.cs`
- In `_PhysicsProcess()`:
  - Check the `rope.ropeState`.
  - If `rope.ropeState == RopeState.Hidden`:
    - Get the global mouse position.
    - Set `monkeArm.GlobalPosition` to the mouse position.
    - Optionally, rotate the arm to face the mouse position for better visual feedback.
  - If `rope.ropeState == RopeState.Shot`:
    - Set `monkeArm.GlobalPosition` to `rope.hookSprite.GlobalPosition` (or the current hook position if accessible).
    - Note: `hookSprite` is in `Rope.cs`, so we might need to expose the hook position or get the node from the rope.
  - If `rope.ropeState == RopeState.Hooked` or `RopeState.Retracting`:
    - Set `monkeArm.GlobalPosition` to the rope's end point (player position) or keep it at the player's hand position.
    - Actually, the user wants it to look like the rope comes out of the arm. So when hooked, the arm should probably be at the player's position, rotated towards the anchor.
    - Let's simplify:
      - **Hidden**: Follow mouse.
      - **Shot**: Follow the moving hook.
      - **Hooked/Retracting**: Follow the player's position (or a fixed offset from player) and rotate towards the anchor.

## Implementation Details
- We need to access the hook's global position from `Rope.cs`. We can add a public property `Vector2 HookPosition` to `Rope.cs` that returns `hookSprite.GlobalPosition`.
- In `player.cs`, we will update `monkeArm.GlobalPosition` and `monkeArm.Rotation` based on the state.
- The arm should be visible only when swinging/airborne as per the previous plan, but now we also want it to aim when hidden. So we might need to adjust the visibility logic.
  - If `rope.ropeState == RopeState.Hidden`, show the arm and make it follow the mouse.
  - If `rope.ropeState != RopeState.Hidden`, show the arm and make it follow the rope/hook.
  - If not swinging/airborne and rope is hidden, hide the arm? Or always show it when aiming?
  - The user said "if the rope is not out, i want the arm to follow the cursor". This implies it should be visible when aiming.
  - So, visibility logic:
    - If `rope.ropeState == RopeState.Hidden`, `monkeArm.Visible = true`.
    - If `rope.ropeState != RopeState.Hidden`, `monkeArm.Visible = true`.
    - Otherwise, `monkeArm.Visible = false`.
    - This effectively means the arm is always visible when the rope is active or aiming.

## Changes to `rope/Rope.cs`
- Add a public property `Vector2 HookPosition` that returns the current global position of the hook (either `hookSprite.GlobalPosition` or the calculated position during shot).
