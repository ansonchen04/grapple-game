# Plan: Level Progression & Menu Updates

## Objective
Update the main menu to include level selection, implement a flag object that triggers level completion upon contact, and create a post-level summary screen with navigation options (Menu, Replay, Next Level).

## Main Menu Updates (`menu/main_menu.tscn` & `MainMenu.cs`)
- Replace the single "Play" button with a dedicated "Level Select" container.
- Add three buttons: "Level 1", "Level 2", "Level 3".
- Initially, only "Level 1" will be functional; L2/L3 buttons will be disabled or show a placeholder state until those levels are built.
- Update `MainMenu.cs` to route button presses to the correct level scene paths.

## Flag Asset (`level/common/Flag.tscn`)
- Create a lightweight flag asset using an `Area2D` as the root node (allows overlap detection without blocking movement).
- Add a `CollisionShape2D` for hitbox detection and a simple visual marker (`Sprite2D` or `ColorRect`).
- Attach `Flag.cs` script to handle `body_entered` signals.
- When the player collides with the flag, it will trigger the level completion transition.

## Level Complete Screen (`menu/LevelComplete.tscn` & `LevelComplete.cs`)
- Create a new UI scene acting as an interstitial screen between levels and the menu.
- Layout: Centered title ("Level Complete!"), followed by three buttons: "Menu", "Replay", "Next Level".
- Script logic:
  - **Menu**: Loads `main_menu.tscn`.
  - **Replay**: Reloads the current level scene.
  - **Next Level**: Placeholder for now (disabled or shows "Coming Soon"), ready to load `level2.tscn` later.
- Will use a simple static reference or scene tree property to track which level was just completed for replay routing.

## Player & Transition Integration
- Leverage the Flag's `Area2D` overlap detection rather than modifying player physics directly.
- Upon flag contact, pause input/game state and call `GetTree().ChangeSceneToFile("res://menu/LevelComplete.tscn")`.
- Ensure camera smoothing and rope state reset cleanly during scene transitions to prevent carry-over bugs.

## Implementation Steps
1. Create `level/common/Flag.tscn` with `Area2D`, collision shape, visuals, and `Flag.cs`.
2. Update `menu/main_menu.tscn` layout and `MainMenu.cs` logic for level selection.
3. Create `menu/LevelComplete.tscn` UI and `LevelComplete.cs` navigation script.
4. Wire up flag detection to trigger the complete screen transition.
5. Test full flow: Main Menu -> Level 1 -> Touch Flag -> Complete Screen -> Replay/Menu.

## Notes
- Keep progression state simple initially (no persistent save system yet).
- "Next Level" will be visually present but functionally inert until L2 is implemented.
- Will rely on Godot's built-in scene changing API for clean state resets.
