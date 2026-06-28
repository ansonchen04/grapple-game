# Grapple System Status & Context
*Last Updated: 2026-06-28 | Godot 4.6 C# | 2D Platformer*

## 🏗 Architecture Overview
- **`rope/Rope.cs`**: Manages state machine (`RopeState`), raycast aiming, Verlet rope simulation (visuals only), and exposes anchor/length data to the player.
- **`player/player.cs`**: Handles character movement, swing physics via `ApplySwingPhysics()`, momentum preservation, and input mapping.
- **`swing_physics_plan.md`**: Documents all implemented steps (1-12) and design rationale.
- **Decoupled Design**: Visual rope (Verlet) is separate from player constraint physics (spring-damper). This prevents Godot's `CharacterBody2D` from fighting rigid-body joints, eliminating jitter.

## ✅ Implemented Features (Steps 1-12 Complete)
1. **Expose Anchor & Rope Data**: `GetAnchor()`, `GetMaxRopeLength()` in `Rope.cs`.
2. **Strictly Unidirectional Constraint**: Spring/damping only activates when `dist > maxLen` AND moving outward. Prevents "stick" suspension.
3. **Screen-Space Input Projection**: Raw horizontal input projected onto tangent plane. Fixes control reversal below anchor.
4. **Gravity & Integration Order**: `Gravity → Spring/Damping → Tangential Input → MoveAndSlide()`.
5. **Clean Release & State Sync**: Velocity preserved on release, Verlet arrays cleared.
6. **Platform Jumping While Hooked**: `IsOnFloor()` check allows jumping mid-swing.
7. **Momentum-Preserving Retraction**: Direct inward force (1500), zero damping, explicit tangential velocity preservation.
8. **Aerial Momentum Preservation**: `baseMovement()` skips air friction when airborne to maintain swing glide.
9. **Dynamic Deployed Length**: Rope locks to exact grapple distance (`deployedLength`), not hardcoded 500px.
10. **Increased Speed Cap**: Raised to 900 px/s (later removed cap entirely in Step 11).
11. **Removed Tangential Speed Cap**: Unlimited swing acceleration for natural momentum buildup.
12. **Dynamic Anchor Tracking**: `anchorNode` + `anchorLocalOffset` tracks moving platforms/elevators.

## 🔑 Key Implementation Details
### Rope.cs
- **Aiming**: Uses `PhysicsRayQueryParameters2D` with `Exclude = { player.GetRid() }` to prevent self-collision. `HitFromInside = true` for edge precision.
- **Verlet**: 30 segments, 15 constraint iterations, 0.96 damping, gravity 400. Updates in `_PhysicsProcess`.
- **Moving Anchors**: `GetAnchor()` returns `anchorNode.GlobalPosition + anchorLocalOffset` if valid, else fallback to `currentAnchor`. Cleared on release.
- **Debug Viz**: `_Draw()` renders cyan ray path, red hit marker, yellow miss marker.

### player.cs
- **Swing Physics**: `ApplySwingPhysics(ref vel, delta)` handles all hooked/retracting logic.
- **Spring Tuning**: `k = 350`, `c = 2√k` (critical damping). Blend window: `1.0x → 1.2x maxLen`. Hard clamp: `1.4x maxLen`.
- **Input Projection**: `Vector2 tangentialInput = inputDir - (inputDir.Dot(radialDir) * radialDir);`
- **Retraction**: `ropeState == Retracting` applies `-radialDir * 1500 * dt`, strips radial velocity, restores tangential velocity.
- **Momentum Preservation**: `baseMovement()` checks `Mathf.Abs(velocity.X) <= speed`. If moving fast in same direction as input, velocity is untouched.

## ⚙️ Current Tuning & Behavior Notes
- Rope feels "stretchy/web-like" due to softened spring (`k=350`) and unidirectional constraint.
- No artificial speed caps during swings; momentum carries naturally into free-fall.
- Retraction pulls smoothly without braking tangential speed.
- Moving platforms are tracked correctly via node reference + local offset.
- Debug visualization remains active in `Hidden`/`Shot` states for aiming feedback.

## 📌 Next Steps / Open Questions
- Consider cleaning up debug visualization (`_Draw()` calls) once aiming is fully validated.
- Tune spring stiffness/damping if "stretchy" feel needs adjustment (currently balanced for web-like elasticity).
- Add sound effects/particles for hook impact, retraction, and release.
- Implement `Slack` state logic if needed (currently unused but in enum).

## 📂 Critical Files to Reference
- `rope/Rope.cs`
- `player/player.cs`
- `swing_physics_plan.md`
- `rope/RopeState.cs`
