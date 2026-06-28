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

## Step 6: Platform Interaction & Jumping While Hooked
- **What:** Allow the player to trigger a normal jump while `RopeState.Hooked` if `IsOnFloor()` returns true. The jump impulse will override vertical velocity temporarily, and the spring constraint will yield to the upward force without detaching.
- **Why:** Players expect to land on platforms mid-swing and immediately jump again. Forcing a manual release before jumping breaks flow and feels clunky in fast-paced platforming.
- **Expected Result:** Land on a platform while swinging, press Up, and launch upward naturally. The rope remains attached until manually released or slack conditions are met, enabling seamless swing-to-platform transitions.

## Step 7: Momentum-Preserving Retraction (Refined)
- **What:** Replace dynamic `MaxLength` shrinking with a direct inward radial force applied during `Retracting`. Completely disable radial damping while retracting to prevent braking. Explicitly project and preserve the tangential velocity vector each frame before applying the retraction force, ensuring horizontal/swing momentum is mathematically isolated from the inward pull.
- **Why:** Shrinking `MaxLength` forces the spring constraint to constantly trigger, and critical damping aggressively removes radial velocity. This acts as a brake on overall speed and inadvertently drains tangential/horizontal momentum due to constraint fighting. Direct force application bypasses solver overhead, while disabling damping guarantees momentum conservation.
- **Expected Result:** Holding retract smoothly pulls you toward the anchor without slowing down your swing. Horizontal/tangential speed is fully preserved (or even boosted by gravity), allowing high-speed launches upon release with zero braking or oscillation.

## Step 8: Aerial Momentum Preservation on Release
- **Diagnosis:** Releasing the grapple hands control back to `baseMovement()`, which unconditionally applies `Mathf.MoveToward(Velocity.X, 0, speed)` when no input is pressed. This acts as extreme artificial air friction, instantly killing horizontal swing momentum mid-air.
- **Fix:** Modify `baseMovement()` (or the transition fallback) to preserve existing horizontal velocity while airborne (`!IsOnFloor()`) if no directional input is active. Only apply deceleration/friction when grounded or when explicitly counter-steering in the air.
- **Expected Result:** Releasing mid-swing will allow the player to coast through the air with full accumulated horizontal speed, matching Spider-Man's aerial glide feel. Momentum transitions seamlessly from swinging to free-fall without artificial braking.

## Step 9: Dynamic Deployed Length & Elastic Tuning
- **What:** Capture the exact distance between player and anchor at the moment of hook (`deployedLength`). Replace the hardcoded `MaxLength` constraint with this dynamic value. Lower spring stiffness (`k`) from `800` to `~350` and recalculate critical damping (`c = 2√k`) to allow noticeable, controlled stretch. Widen the force blend window and hard clamp threshold slightly to safely accommodate the softer elasticity.
- **Why:** The current system targets a fixed 500px radius regardless of actual grapple distance, causing short grapples to feel artificially long or completely slack until extreme momentum is reached. Capturing the deployed length ensures the rope behaves like a true tether. Softening the spring parameters introduces that desired "web-like" elasticity without sacrificing numerical stability.
- **Expected Result:** Grappling at any distance locks the rope to that exact length. Fast swings will visibly stretch the rope, storing kinetic energy, then smoothly snap back to the deployed length as momentum decreases. No more artificial extension or dead zones.

## Step 10: Increased Tangential Speed Cap for High-Velocity Swings
- **What:** Increase the `maxTangentialSpeed` cap from `600 px/s` to `900 px/s`. Adjust the acceleration falloff curve proportionally to maintain control at higher speeds while allowing players to reach greater velocities during long drops or rapid retraction chains.
- **Why:** The previous cap felt restrictive for high-momentum swings, artificially braking the player before they could fully utilize gravity or retraction boosts. Raising the limit enables faster, more dynamic aerial maneuvers without compromising stability, as the critically damped spring system will still safely manage extreme radial forces.
- **Expected Result:** Swings feel significantly faster and more exhilarating. Players can build up substantial speed during long arcs or rapid retracts, enabling high-velocity launches and advanced platforming techniques.

## Why This Works
Spring-damper systems are mathematically stable in game loops because they convert kinetic energy into potential energy smoothly. By damping radial velocity, we prevent infinite bouncing while preserving the "stretchy" web feel. Tangential input mapping aligns player control with the physics of swinging, and preserving aerial inertia on release ensures momentum conservation across state transitions, resulting in a responsive, professional-grade grapple mechanic.
