# Shooting Hook Implementation Plan

## Overview
This plan outlines how we will implement a visible, projectile-style grappling hook that shoots from the player's position. The hook will travel along the aim line at a fixed speed, draw a trailing rope behind it, and only attach if it successfully hits a valid collider. If it reaches maximum range without hitting anything, it will automatically retract and reset to the hidden state.

## Step 1: Input State Transition (`Rope.cs`)
- **Action:** Modify `_Input` so that pressing Left Click while in `Hidden` state transitions to `Shot`.
- **Details:** Cache the aim direction, total shot distance, and crucially, the hit status (`hasLastHit`) and target position from the current raycast. Disable aim updates during flight to prevent jitter.

## Step 2: Projectile Animation & Tracking (`Rope.cs`)
- **Action:** Add a dedicated update routine for the `Shot` state in `_Process`.
- **Details:** Move the `HookSprite` from the player's fire position toward the cached target using constant velocity. Track distance traveled vs. total shot distance. Keep the hook visible and disable debug aim lines during this phase.

## Step 3: Hit Resolution & Conditional Hooking (`Rope.cs`)
- **Action:** When the hook reaches its maximum travel distance, evaluate the cached hit status before committing to a swing.
- **Details:** 
  - **If `hasLastHit` is true:** Transition to `Hooked`, set `currentAnchor` to the hit point, and call `InitializeRope()`. The Verlet rope will instantly generate from the player to the newly anchored hook.
  - **If `hasLastHit` is false (max distance reached):** Immediately skip rope initialization and transition back to `Hidden`. This guarantees the hook never attaches to empty space.

## Step 4: Dynamic Line Drawing (`Rope.cs`)
- **Action:** Update `_Draw()` to reflect the shooting phase.
- **Details:** While in `Shot`, draw a simple line from the player's current position to the moving `HookSprite`. Once hooked, hand off rendering to the existing Verlet segment array. This maintains visual continuity without complex rope-phasing logic.

## Step 5: Player Movement Gating (`player.cs`)
- **Action:** Ensure `Shot` is treated as a non-swinging state.
- **Details:** The existing `isSwinging` check gates on `Hooked`/`Retracting`. We'll verify that `Shot` falls through to normal gravity/base movement, so the player can still jump, climb, or move freely while the hook is in flight.

## Step 6: Automatic Retraction & Abort Handling (`Rope.cs`)
- **Action:** Handle both manual aborts and automatic misses during the shot using a unified cleanup routine.
- **Details:** If Left Click is pressed again, Right Click is used, or the player restarts while in `Shot`, immediately cancel the animation, hide the hook, clear cached shot data, and return to `Hidden`. The automatic retraction on a miss (from Step 3) will trigger this exact same cleanup logic to ensure no phantom ropes, stuck states, or lingering physics constraints remain.

## 🎯 Expected Result
Clicking will fire a visible hook projectile along your aim line. It travels at a fixed speed with a trailing line behind it. If it hits a valid surface, it snaps into a fully simulated stretchy rope. If it reaches max range without hitting anything, it instantly vanishes and resets to `Hidden` state. Movement remains uninterrupted during flight, and aborting mid-shot cleanly resets everything.
