# Swing Physics Implementation Plan (Stretchy Grapple)

## Overview
This plan outlines how we will implement smooth, responsive swinging mechanics using a spring-based attachment system instead of rigid distance clamping. This approach mimics the "stretchy" feel of Spider-Man's webs, allowing natural momentum buildup while preventing jittery snapping.

## Step 1: Expose Anchor & Rope Data
- **What:** Add getter methods to `Rope.cs` (`GetAnchor()`, `GetMaxRopeLength()`).
- **Why:** `player.cs` needs deterministic access to the hook position and rope limits every physics frame.
- **Expected Result:** No visual change yet, but player script can query exact swing boundaries.

## Step 2: Critically Damped Spring Attachment (Stretchy Grapple)
- **What:** Implement a "rope" constraint (zero force inside radius, restoring force only past `MaxLength`). Use a critically damped oscillator (`c = 2 * sqrt(k)`) instead of independent stiffness/damping knobs to guarantee numerical stability. Blend the spring force over a 10% window past `MaxLength` to eliminate velocity "pops". Add a hard position clamp at `1.3x MaxLength` as a safety backstop against high-speed overshoot.
- **Why:** Bare Hooke springs integrated with Euler steps can oscillate or blow up if tuned poorly. Critical damping collapses tuning into one predictable pair, while the blend window and hard clamp prevent edge-case tunneling or infinite stretch.
- **Expected Result:** The rope stretches organically under momentum but settles instantly without bouncing. Fast swings feel elastic but never break physics bounds.

## Step 3: Tangential Input Mapping with Speed Cap
- **What:** While hooked, remap horizontal input to apply acceleration along the tangent vector of the swing arc. Apply a velocity-dependent acceleration falloff and hard cap at `600 px/s` tangential speed to prevent infinite energy pumping.
- **Why:** Standard cardinal movement feels disconnected when swinging. Tangential mapping ensures input directly influences swing momentum, while the speed cap prevents players from accelerating uncontrollably at the bottom of long arcs.
- **Expected Result:** Pressing left/right smoothly accelerates or decelerates your swing along the rope's path, giving tight aerial control without breaking physics stability.

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
