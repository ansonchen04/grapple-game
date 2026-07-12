# Plan: One-Way Platforms

## Goal
Implement one-way platforms that the player can jump through from below but land on from above. The player should be able to drop through them by pressing the "Down" key.

## Implementation Steps

1. **Update `player.cs`**:
   - Add a `RayCast2D` to detect if the player is standing on a one-way platform.
   - Add a boolean flag `onOneWaySurface` to track if the player is currently on such a platform.
   - Modify `_PhysicsProcess` to:
     - Check if the player is on a one-way platform using the raycast.
     - If `onOneWaySurface` is true and the player presses "Down", move the player down slightly to break the collision and allow falling through.
     - Ensure gravity and movement work correctly when on a one-way platform.

2. **Create a One-Way Platform Template**:
   - Create a new scene `template/OneWayPlatformTemplate.tscn` with:
     - A `StaticBody2D` as the root node.
     - A `CollisionShape2D` with `OneWayCollision` enabled.
     - A `ColorRect` for visual representation (orange, similar to other platforms).
   - This template can be reused across levels.

3. **Add One-Way Platforms to Level 2**:
   - Add instances of the one-way platform template to `level/level_2/level2.tscn` in appropriate locations.

## Notes
- The `OneWayCollision` property in Godot's `CollisionShape2D` handles the one-way behavior automatically.
- The player's raycast will detect the platform, and the "Down" key input will allow dropping through.
- Ensure the one-way platforms are visually distinct (e.g., orange) to indicate their special behavior.
