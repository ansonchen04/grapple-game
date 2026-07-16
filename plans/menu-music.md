# Plan: Add Music to Main Menu

## Goal
Add background music to the Main Menu scene using the existing audio implementation pattern from the player.

## Files to Modify
1. `menu/main_menu.tscn`
2. `menu/MainMenu.cs`

## Steps

### 1. Update `menu/main_menu.tscn`
- Add an `AudioStreamPlayer` node as a child of the `MainMenu` node.
- Name the node `MenuMusic`.
- Set its `stream` property to `res://music/menu.wav`.
- Ensure the node is visible/active in the scene tree.

### 2. Update `menu/MainMenu.cs`
- Add a private field `AudioStreamPlayer menuMusic`.
- In `_Ready()`:
  - Get the `MenuMusic` node using `GetNode<AudioStreamPlayer>("MenuMusic")`.
  - Call `menuMusic.Play()` to start the music when the menu loads.
- (Optional but recommended) Consider stopping the music when leaving the menu (e.g., in `OnPlayPressed` or `OnQuitPressed`) to prevent it from playing over other scenes, though Godot typically handles scene cleanup. For simplicity, we'll just start it in `_Ready`.

## Implementation Details
- The `AudioStreamPlayer` will automatically loop if the audio resource is set to loop, or we can set `Loop` to `true` in the scene or code if needed.
- No changes are needed to other files.
