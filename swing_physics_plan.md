# Swing Physics Implementation Plan (Stretchy Grapple)

## Overview
This plan outlines how we will implement smooth, responsive swinging mechanics using a spring-based attachment system instead of rigid distance clamping. This approach mimics the "stretchy" feel of Spider-Man's webs, allowing natural momentum buildup while preventing jittery snapping.

## Step 1: Expose Anchor & Rope Data
- **What:** Add getter methods to `Rope.cs` (`GetAnchor()`, `GetMaxRopeLength()`).
- **Why:** `player.cs` needs deterministic access to the hook position and rope limits every physics frame.
- **Expected Result:** No visual change yet, but player script can query exact swing boundaries.

## Step 2: Spring-Based Attachment (Stretchy Grapple)
- **What:** Replace hard distance clamping with a spring-damper system. When player distance > max length, apply a restoring force proportional to the stretch amount. Heavily dampen radial velocity to control oscillation.
- **Why:** Hard constraints fight Godot's `CharacterBody2D` movement, causing micro-collisions and jitter. Springs absorb momentum naturally and feel organic.
- **Expected Result:** The rope will stretch slightly under fast swings or gravity, then smoothly pull the player back. No snapping or position teleportation.

## Step 3: Tangential Input Mapping
- **What:** While hooked, remap horizontal input (`Left`/`Right`) to apply acceleration along the tangent vector of the swing arc.
- **Why:** Standard cardinal movement feels disconnected when swinging. Tangential mapping ensures input directly influences swing momentum.
- **Expected Result:** Pressing left/right smoothly accelerates or decelerates your swing along the rope's path, giving tight aerial control.

## Step 4: Gravity & Momentum Handling
- **What:** Allow Godot's default gravity to act on the player normally. The spring force only activates when stretching occurs. Preserve all tangential velocity during swings.
- **Why:** Pendulum physics rely on gravity for natural arc motion. Letting the engine handle gravity avoids complex manual calculations and keeps movement consistent with platforming.
- **Expected Result:** Smooth pendulum arcs, natural speed buildup at the bottom of swings, and seamless transitions between swinging and falling.

## Step 5: Clean Release & State Sync
- **What:** On release (Left Click), switch state to `Hidden`, clear Verlet arrays, but retain player velocity. Spring forces immediately deactivate.
- **Why:** Releasing mid-swing should launch the player forward with accumulated momentum, not drop them vertically.
- **Expected Result:** Click to release and watch the player arc gracefully into the air, ready to jump or land normally.

## Why This Works
Spring-damper systems are mathematically stable in game loops because they convert kinetic energy into potential energy smoothly. By damping radial velocity, we prevent infinite bouncing while preserving the "stretchy" web feel. Tangential input mapping aligns player control with the physics of swinging, resulting in a responsive, professional-grade grapple mechanic.
