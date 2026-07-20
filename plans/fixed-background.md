# Fixed Background Image Plan

## Goal
Replace the large scaled `Sprite2D` background with a system that keeps the reactor image in full view at all times, behind all other game elements.

## Current State
- `Sprite2D3` in `level1.tscn` uses `sprites/reactor.png` with a large scale (9.037037, 9.037036)
- The sprite is positioned at (3373, 172.00049) with z_index = -3
- As the camera moves, the background scrolls/moves with the world

## Proposed Solution

### Option 1: Camera2D Background (Recommended)
Godot 4.x supports setting a background image directly on the Camera2D node.

**Steps:**
1. Remove `Sprite2D3` from `level1.tscn`
2. In `player/player.cs`, access the `Camera2D` node in `_Ready()`
3. Set the camera's background image to `sprites/reactor.png`
4. The camera will automatically keep the background fixed and in full view

**Pros:**
- Native Godot feature, no custom code needed
- Automatically handles aspect ratio and positioning
- Always stays behind everything
- No performance overhead from extra nodes

**Cons:**
- Background is tied to the camera, not the scene (but this is what we want)

### Option 2: CanvasLayer with Fixed Position
Create a `CanvasLayer` with a `Sprite2D` that doesn't move with the camera.

**Steps:**
1. Create a new `CanvasLayer` node in `level1.tscn`
2. Add a `Sprite2D` child with the reactor texture
3. Set the sprite to fill the screen using viewport size
4. Ensure z-index is lowest

**Pros:**
- More control over positioning
- Can be reused across levels

**Cons:**
- More manual setup
- Need to handle viewport resizing

## Recommendation
Use **Option 1** (Camera2D background) as it's the simplest and most robust solution.

## Implementation Steps
1. Remove `Sprite2D3` node from `level1.tscn`
2. In `player/player.cs`, add code in `_Ready()` to:
   - Get the `Camera2D` node
   - Load the reactor texture
   - Set it as the camera's background image
3. Test to ensure the background stays fixed and in full view
