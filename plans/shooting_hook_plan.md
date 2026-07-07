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

---

## 🔧 Fix Plan: Eliminate Moving Platform Anchor Drift

### 🔍 Diagnosis
The "hooking off" issue on moving platforms is caused by a **timing mismatch between cached aim data and platform movement**. 
1. **Frozen Aim Data:** When you click to fire, `UpdateAim()` runs one last time and caches the collision point (`lastValidHit`) and the hit collider (`lastHitCollider`). During the shot's flight phase, `_Process` stops updating aim data, so these cached values remain frozen at their initial coordinates.
2. **Platform Drift During Flight:** While the hook sprite travels along its fixed trajectory, the platform continues to move. By the time `shotTraveled >= shotDistance`, the actual surface has shifted away from the cached `lastValidHit` position.
3. **Incorrect Local Offset Baking:** In `UpdateShot()`, when the hook reaches max range, it executes `anchorLocalOffset = anchorNode.ToLocal(currentAnchor);`. Because `currentAnchor` is set to the stale `lastValidHit`, this calculates a local offset relative to where the platform *was*, not where it *is*. This "bakes" the positional drift directly into the offset variable.
4. **Compounding Error in Physics Loop:** In `_PhysicsProcess()`, the dynamic anchor tracking logic runs `currentAnchor = anchorNode.ToGlobal(anchorLocalOffset);`. Since `anchorLocalOffset` contains the baked error, every physics frame reconstructs the global anchor position using the wrong local coordinate. The rope consistently attaches slightly off from the actual geometry, and the offset persists for the entire swing.

**Root Cause:** Calculating `anchorLocalOffset` at the end of the shot using a stale world position instead of capturing it relative to the platform's transform at the exact moment of impact (or re-evaluating it dynamically).

### 📋 Implementation Steps

#### Step 1: Cache Local-Space Hit Position (`UpdateAim`)
- **Action:** Modify the raycast hit resolution in `UpdateAim()` to store the collision point in the platform's local coordinate space, not just world space.
- **Details:** Add a new field `Vector2 lastValidLocalHit`. Whenever `result.Count > 0`, compute `lastValidLocalHit = lastHitCollider.ToLocal(debugHit)`. This captures exactly where on the mesh the hook will attach, independent of where the platform moves later.

#### Step 2: Apply Local Offset Directly on Impact (`UpdateShot`)
- **Action:** Update the hit resolution logic in `UpdateShot()` to bypass the stale world-position calculation entirely.
- **Details:** When `shotTraveled >= shotDistance` and `hasLastHit` is true, assign `anchorLocalOffset = lastValidLocalHit` directly. Do not call `ToLocal()` on a frozen world position, as that bakes the flight-time drift into the offset.

#### Step 3: Re-derive Global Anchor Position
- **Action:** Immediately calculate the accurate global anchor using the fresh local offset.
- **Details:** Set `currentAnchor = anchorNode.ToGlobal(anchorLocalOffset)`. This forces the rope to sync with the platform's exact transform at the moment of impact, guaranteeing pixel-perfect attachment even if the platform moved significantly while the hook was flying.

### 🎯 Expected Result
The grappling hook will attach to the precise surface coordinate on moving platforms, completely eliminating the visual/physical offset caused by platform movement during the shot phase. Swings will remain perfectly anchored regardless of elevator speed or animation timing, with zero baked drift.
