# Swing Physics Implementation Plan (Stretchy Grapple)

## Overview
This plan outlines how we will implement smooth, responsive swinging mechanics using a spring-based attachment system instead of rigid distance clamping. This approach mimics the "stretchy" feel of Spider-Man's webs, allowing natural momentum buildup while preventing jittery snapping.

## Step 1: Expose Anchor & Rope Data
- **What:** Add getter methods to `Rope.cs` (`GetAnchor()`, `GetMaxRopeLength()`).
- **Why:** `player.cs` needs deterministic access to the hook position and rope limits every physics frame.
- **Expected Result:** No visual change yet, but player script can query exact swing boundaries.

## Step 2: Strictly Unidirectional Rope Constraint (No "Stick" Behavior)
- **What:** Enforce a strictly unidirectional pull. The spring/damping system will **only** activate when `dist > MaxLength` AND radial velocity is positive (moving outward). If the player is moving inward (`radialVel < 0`) or within `MaxLength`, all constraint forces are disabled completely, allowing gravity and existing momentum to dominate freely. This prevents the "rigid stick" suspension effect when jumping above the anchor.
- **Why:** Real ropes go slack under compression/inward motion. Applying damping or spring forces during inward movement artificially resists gravity, causing the player to float or hang unnaturally. Strict unidirectionality guarantees natural pendulum arcs and free-fall transitions.
- **Expected Result:** The rope pulls only when stretched outward. When moving inward or above the anchor, the player falls freely under gravity until the rope naturally tightens again. No artificial suspension or stick-like rigidity.

## Step 3: Screen-Space Tangent Mapping & Pole Safety
- **What:** Recalculate the tangent vector to align with screen-space left/right expectations in Godot's Y-down coordinate system. Add a small epsilon check (`dist > 0.1f`) and clamp tangent calculations when directly above/below the anchor to prevent division-by-zero or NaN freezes at vertical extremes. Maintain velocity-dependent acceleration falloff and `600 px/s` cap.
- **Why:** Standard mathematical tangents often invert visually in Y-down engines, causing left/right input swaps. Vertical alignment (directly over/under anchor) creates degenerate tangent vectors that can freeze movement or cause erratic snapping. Explicit screen-space mapping and pole safety ensure consistent control across all swing angles.
- **Expected Result:** Left/right inputs correctly accelerate/decelerate the swing along the arc in all directions. No input inversion, no freezing at vertical extremes, and smooth momentum buildup throughout the full 360° swing range.

## Step 4: Gravity & Momentum Handling (Strict Integration Order)
- **What:** Enforce strict per-frame integration order: `Gravity → Spring/Damping Correction → Tangential Input → MoveAndSlide()`. Preserve tangential velocity by projecting out radial components before applying constraints. Anchor semantics are fixed-world points for now.
- **Why:** Pendulum physics rely on gravity for natural arc motion. Strict ordering prevents input from leaking into the spring correction next frame, and fixed anchors simplify constraint math while keeping `MoveAndSlide()` collision resolution intact.
- **Expected Result:** Smooth pendulum arcs, natural speed buildup at the bottom of swings, and seamless transitions between swinging and falling. The stretchy feel won't interfere with normal jumping or landing.

## Step 5: Clean Release & State Sync
- **What:** On release (Left Click), switch state to `Hidden`, clear Verlet arrays, but retain player velocity. Spring forces immediately deactivate.
- **Why:** Releasing mid-swing should launch the player forward with accumulated momentum, not drop them vertically.
- **Expected Result:** Click to release and watch the player arc gracefully into the air, ready to jump or land normally.

## Why This Works
Spring-damper systems are mathematically stable in game loops because they convert kinetic energy into potential energy smoothly. By damping radial velocity, we prevent infinite bouncing while preserving the "stretchy" web feel. Tangential input mapping aligns player control with the physics of swinging, resulting in a responsive, professional-grade grapple mechanic.
