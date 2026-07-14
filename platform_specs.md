# Platform Specifications

| File | Node Name | Shape Type | Dimensions (in px) | Effective Size (if scaled) | Sprite/Texture Ref | Tiling (Y/N) | Notes |
|------|-----------|------------|-------------------|---------------------------|-------------------|--------------|-------|
| level/level_2/Lvl2SmallPlatform.tscn | Lvl2SmallPlatform | RectangleShape2D | 367.806 x 119.074 | 367.806 x 119.074 | ColorRect (no texture) | N | None |
| level/level_1/Lvl1SmallPlatform.tscn | StartingPlatform | CollisionPolygon2D | Polygon: (-17.7, 11), (17.7, 11), (9.6, 56), (-1.4, 84.9), (-10.9, 56) | Scaled by (6.298, 1) on node | Polygon2D (no texture) | N | Scale applied to collision and sprite nodes |
| level/common/SpikePlatform.tscn | SpikePlatform | RectangleShape2D | 578 x 64 | 578 x 64 | ColorRect (no texture) | N | Contains 9 nested HazardSpikes instances |
| player/player.tscn | Player | RectangleShape2D | 128 x 128 | 128 x 128 | Sprite2D: res://icon.svg | N | Player character, not a platform |
| level/level_2/Lvl2MovingPlatform.tscn | PlatformBody | RectangleShape2D | 367.806 x 119.074 | ~363.41 x ~120.54 (scaled by 0.988, 1.012) | ColorRect (no texture) | N | Complex hierarchy with Path2D/PathFollow2D/RemoteTransform2D |
| level/level_1/Lvl1StartingPlatform.tscn | StartingPlatform | CollisionPolygon2D | Polygon: (-146.7, 5), (146.7, 1), (92.6, 645), (-1.7, 806), (-107.3, 643) | Scaled by (6.298, 1) on node | Polygon2D (no texture) | N | Scale applied to collision and sprite nodes |
| level/common/SwingAnchor.tscn | SwingAnchor | CircleShape2D | Radius: 96.0 | Radius: 96.0 | Sprite2D: res://icon.svg (scale 1.5) | N | Used for rope swinging |
| level/level_3/Lvl3DPlatform.tscn | Lvl3DPlatform | CollisionPolygon2D | Polygon: (0, -64), (-256, -64), (-256, 64), (0, 128), (256, 64), (256, -64) | Same as dimensions | Polygon2D (no texture) | N | Diamond/trapezoid shape |
| level/common/Flag.tscn | Flag | RectangleShape2D | 32 x 64 | 32 x 64 | ColorRect (no texture) | N | Area2D, not a platform |
| rope/hook.tscn | Hook | CircleShape2D | Radius: 1.0 (default) | Radius: 1.75 (scaled by 1.75) | Sprite2D: res://icon.svg (scale 0.25) | N | RigidBody2D, contains PinJoint2D |
| level/common/Checkpoint.tscn | Checkpoint | RectangleShape2D | 32 x 64 | 32 x 64 | ColorRect (no texture) | N | Area2D, not a platform |
| level/common/HazardSpikes.tscn | HazardSpikes | RectangleShape2D | 64 x 64 | 64 x 64 | Polygon2D (no texture) | N | Area2D, hazard |
| template/OneWayPlatformTemplate.tscn | OneWayPlatform | RectangleShape2D | 367.806 x 59.537 | 367.806 x 59.537 | ColorRect (no texture) | N | one_way_collision = true |
