# Plan: Hazard Spikes

## Goal
Create hazard spikes that kill the player (restart at last checkpoint) upon contact. The spikes should be visually represented as triangles and can be placed on surfaces or floating.

## Implementation Steps

1. **Create `HazardSpikes.cs` Script**:
   - Inherit from `Area2D` to detect player entry.
   - Add a `BodyEntered` signal handler.
   - When the player enters:
     - Call `player.restart()` to reset the player to the last checkpoint.
     - Optionally add a small delay or visual effect before restarting.

2. **Create `HazardSpikes.tscn` Scene**:
   - Root node: `Area2D` with `HazardSpikes.cs` attached.
   - Add a `CollisionShape2D` with a `TriangleShape2D` or `PolygonShape2D` for the spike shape.
   - Add a `Polygon2D` or `ColorRect` for visual representation (red triangles).
   - Ensure `monitoring` is true and `monitorable` is false.

3. **Add to Levels**:
   - Place hazard spikes in `level/level_1/level1.tscn` and `level/level_2/level2.tscn` in appropriate locations (e.g., on ceilings, floors, or floating).

## Notes
- The spikes should be visually distinct (e.g., red) to indicate danger.
- The collision shape should match the visual shape for accurate detection.
- The restart behavior should be consistent with out-of-bounds death.
