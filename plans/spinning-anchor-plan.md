# Plan: Spinning Anchor

## Goal
Create a spinning anchor object that rotates clockwise around its center. The player should be able to grapple onto it, and the rope anchor point should update dynamically as the anchor spins.

## Implementation Steps

1. **Create `SpinningAnchor.cs` Script**:
   - Inherit from `Node2D` (similar to `SwingAnchor`).
   - Add an `[Export]` property for `RotationSpeed` (in radians per second).
   - In `_Process(double delta)`, update the node's rotation: `Rotation += RotationSpeed * (float)delta`.
   - Ensure the visual representation (e.g., a `Sprite2D` or `ColorRect`) rotates with the node.

2. **Create `SpinningAnchor.tscn` Scene**:
   - Root node: `Node2D` with `SpinningAnchor.cs` attached.
   - Add a `Sprite2D` or `ColorRect` for visual feedback (e.g., a circular shape or a distinct icon).
   - Add a `CollisionShape2D` with a `CircleShape2D` so the player can grapple onto it.
   - Set `collision_mask` to `1` (platform layer) so the rope raycast can detect it.

3. **Integrate with Rope System**:
   - The existing `Rope.cs` already handles dynamic anchor tracking via `anchorNode` and `anchorLocalOffset`.
   - Since `SpinningAnchor` is a `Node2D`, the rope will automatically track its global position and local offset as it rotates.
   - No changes needed to `Rope.cs` or `player.cs` for basic functionality.

4. **Add to Level 2**:
   - Place one or more spinning anchors in `level/level_2/level2.tscn` in strategic locations (e.g., above gaps or as part of a puzzle).

## Notes
- The rotation should be smooth and continuous.
- The collision shape should be circular to allow grappling from any angle.
- The visual should clearly indicate the spinning motion (e.g., a rotating arrow or distinct shape).
