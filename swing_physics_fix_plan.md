# Swing Physics Restoration Plan

## 🔍 Diagnosis of Regression
The recent unification of the movement system introduced subtle but impactful changes to force accumulation and integration order, altering the swing feel:
1. **Horizontal Velocity Overriding**: `baseMovement()` runs every frame before rope constraints. When horizontal speed drops below walk speed (`300 px/s`), it forcibly clamps `velocity.X`, instantly killing natural pendulum momentum. Even at high speeds, raw horizontal input is applied twice (once in `baseMovement`, again via tangential projection in `ApplyRopeConstraints`), causing twitchy/over-sensitive controls.
2. **Gravity Integration Order Shift**: Gravity was moved from inside the swing solver to the top of `_PhysicsProcess`. Applying it before relative velocity calculation changes how the spring/damper reacts to vertical momentum mid-swing, altering the arc's natural weight and damping.
3. **Friction Scope Expansion**: Surface friction is now applied globally every frame instead of only during swings. This introduces unintended air resistance or wall-sliding damping when ungrappled, and changes how momentum bleeds off during state transitions.
4. **Jump Logic Duplication**: Jump handling exists in both `baseMovement()` and the main loop's step 5. When hooked, this can cause vertical velocity conflicts or accidental double-jumps.

## 📋 Implementation Plan

### Step 1: Restore Original `ApplySwingPhysics` Logic
- **Action**: Revert `ApplyRopeConstraints` back to the original `ApplySwingPhysics` implementation that handled gravity internally, managed tangential input projection, and applied surface friction only during swings.
- **Why**: Guarantees the spring/damper math, retraction force, anchor velocity compensation, and momentum preservation behave identically to the pre-unification state.

### Step 2: Gate `baseMovement` During Swings
- **Action**: Modify `_PhysicsProcess` so that when `ropeState == Hooked || Retracting`, it completely bypasses `baseMovement()`'s horizontal overrides.
- **Why**: Prevents walk-speed clamping from interfering with pendulum momentum and stops double-input application. Ungrappled movement will still flow through `baseMovement` normally.

### Step 3: Fix Gravity Double-Application
- **Action**: Add a conditional check in the main loop: only apply gravity at the top of `_PhysicsProcess` if *not* swinging. Since the restored swing solver will handle its own gravity step, this prevents vertical force stacking.
- **Why**: Maintains deterministic integration order without altering the tuned pendulum weight.

### Step 4: Consolidate Jump Handling
- **Action**: Remove jump logic from `baseMovement()` and keep a single, authoritative jump check at the end of `_PhysicsProcess`.
- **Why**: Ensures jumping works seamlessly whether hooked or ungrappled, without state conflicts or duplicate vertical overrides.

### Step 5: Localize Friction to Swing States
- **Action**: Keep surface friction scoped inside the restored swing solver rather than applying it globally in `_PhysicsProcess`.
- **Why**: Matches previous tuning and prevents unintended damping during free-fall or grounded movement.

### Step 6: Unify Airborne Input Acceleration
- **Diagnosis**: `baseMovement()` instantly assigns horizontal velocity to walk speed (`velocity.X = direction.X * speed`) when airborne, creating snappy, direct control. Meanwhile, `ApplySwingPhysics()` applies continuous tangential acceleration (`relVel += ... * delta`), causing gradual momentum buildup. This mismatch makes hooking/releasing feel jarring as control responsiveness suddenly shifts.
- **Action**: Refactor the airborne branch of `baseMovement()` to use an acceleration-based model instead of direct velocity assignment. Apply horizontal input as a force delta scaled by `delta`, matching the magnitude and curve of the swing's tangential input. Optionally introduce consistent air drag or a soft max-speed cap to prevent infinite acceleration, ensuring both states build momentum at identical rates.
- **Why**: Aligns the control curve across all airborne states. Players won't experience sudden "snappy" vs "floaty" shifts when transitioning between swinging and free-fall.
- **Expected Result**: Consistent air acceleration and momentum buildup whether grappled or unhooked. Smooth, predictable transitions with identical input feel across all aerial maneuvers.

## 🎮 Expected Results Post-Fix
- **Restored Swing Feel**: Spring stiffness, damping, retraction force, and tangential input projection will behave exactly as before.
- **Clean Unified Loop**: The architecture remains a single `_PhysicsProcess` loop, but state-specific logic is properly gated to prevent interference.
- **Seamless Transitions**: Hooking/releasing will smoothly hand off momentum without velocity snaps or artificial braking.
- **Consistent Controls**: Input acceleration will only apply once per frame, eliminating twitchiness while preserving high-speed swing pumping.

## ✅ Next Steps
1. Apply changes to `player/player.cs`.
2. Test hook/release cycles, mid-air retraction, and platform landings.
3. Verify momentum transfers smoothly across all states.
