# Plan: Level 1 Tutorial Design

## Objective
Design `level/level_1/level1.tscn` as a progressive tutorial that introduces basic movement, jumping, and grappling mechanics in a safe, guided environment.

## Current Assets & Physics Context
- **Platforms**: `Lvl1StartingPlatform.tscn` (large), `Lvl1SmallPlatform.tscn` (small). Both use orange-ish color.
- **Player Physics**: `speed = 300`, `jumpVelocity = -600`, gravity ~980. Max jump height ~185px, max horizontal jump distance ~250px.
- **Grapple**: Max length 500px, raycast detection, swing physics active.

## Level Layout Strategy
### Section 1: Movement & Jumping Basics (X: 0 to 400)
- Keep the large starting platform near X=0.
- Place 2 small platforms at X=250 and X=450, slightly elevated or at same height.
- Spacing (~200px gaps) allows comfortable jumps without grappling, teaching WASD/Arrow movement and jump timing.

### Section 2: Grapple Introduction (X: 450 to 800)
- Place a high anchor platform at X=650, Y=-150 (relative to start). Too high for a normal jump.
- Add a wide gap (~350px) after it that requires swinging across.
- Visual cue: The aim line will naturally guide the player to click on the high platform.

### Section 3: Swing & Momentum Practice (X: 800 to 1200)
- A sequence of 2 platforms requiring a swing to gain height/momentum, then landing.
- Teaches releasing the grapple at the right time and combining air control with swinging.

### Section 4: Goal Platform (X: 1300+)
- Create a distinct "Goal" platform template (`Lvl1GoalPlatform.tscn`) with a green/gold color to signify completion.
- Place it after the final swing challenge.

## Implementation Steps
1. **Create Goal Platform Template**: Duplicate `Lvl1SmallPlatform.tscn`, change polygon color to Green (`Color(0, 1, 0, 1)`), save as `Lvl1GoalPlatform.tscn`.
2. **Update Level Scene**: 
   - Clear/replace existing placeholder platforms in `level1.tscn`.
   - Instantiate and position platforms according to the layout strategy.
   - Adjust Player start position to stand safely on the first platform.
3. **Tune Spacing**: Verify jump distances against player velocity curves. Ensure grapple targets are within 500px raycast range.
4. **Test & Iterate**: Run level, check for phasing, verify tutorial flow feels natural.

## Notes
- No new scripts required; relies entirely on existing `player.cs` and `Rope.cs`.
- Will use Godot's scene instantiation in `.tscn` format.
- Spacing will be approximate initially, ready for fine-tuning after implementation.
