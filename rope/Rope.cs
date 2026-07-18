using Godot;
using System;

public partial class Rope : Node2D {
	CharacterBody2D player;
	public RopeState ropeState;

	const float MaxLength = 500.0f;
	
	Vector2[] positions;
	Vector2[] previousPositions;
	const int MaxSegments = 30;

	Line2D ropeLine;
	Sprite2D hookSprite;
	Sprite2D aimSprite;
	AudioStreamPlayer shootSFX;
	float SegmentLength; 
	const float RopeGravity = 200.0f; // Reduced to prevent excessive sagging that exceeds the physics slack buffer
	bool _isRopeInitialized = false;
	Vector2 currentAnchor = Vector2.Zero;
	
	// Debug visualization & aim caching
	Vector2 debugOrigin = Vector2.Zero;
	Vector2 debugEnd = Vector2.Zero;
	Vector2 debugHit = Vector2.Zero;
	bool hasHit = false;
	
	Vector2 lastValidHit = Vector2.Zero;
	Vector2 lastValidLocalHit = Vector2.Zero;
	bool hasLastHit = false;

	public Vector2 GetAnchor() => currentAnchor;
	public float GetMaxRopeLength() => deployedLength;
	public Vector2 GetHookPosition() => hookSprite.GlobalPosition;
	float deployedLength = MaxLength;
	Node2D anchorNode = null;
	Vector2 anchorLocalOffset = Vector2.Zero;
	Node2D lastHitCollider = null;

	// Shooting hook state
	Vector2 shotOrigin = Vector2.Zero;
	Vector2 shotDirection = Vector2.Zero;
	float shotDistance = 0f;
	float shotTraveled = 0f;
	const float ShotSpeed = 1500.0f;
	bool isShooting = false;

	public override void _Ready() {
		player = GetNode<CharacterBody2D>("../Player");
		ropeLine = GetNode<Line2D>("RopeLine");
		hookSprite = GetNode<Sprite2D>("HookSprite");
		aimSprite = GetNode<Sprite2D>("AimSprite");
		shootSFX = GetNode<AudioStreamPlayer>("ShootSFX");
		ropeState = RopeState.Hidden;
		
		positions = new Vector2[MaxSegments + 1];
		previousPositions = new Vector2[MaxSegments + 1];
	}

	public CharacterBody2D GetPlayer() => player;

	public override void _Process(double delta) {
		if (ropeState == RopeState.Hidden) {
			UpdateAim();
		} else if (ropeState == RopeState.Shot) {
			UpdateShot(delta);
		} else {
			hookSprite.Visible = false;
			aimSprite.Visible = false;
		}
	}

	public override void _PhysicsProcess(double delta) {
		if (ropeState == RopeState.Hooked || ropeState == RopeState.Retracting) {
			// Dynamic anchor tracking: update global position based on moving platform
			if (anchorNode != null && GodotObject.IsInstanceValid(anchorNode)) {
				currentAnchor = anchorNode.ToGlobal(anchorLocalOffset);
			} else {
				anchorNode = null; // Platform destroyed or left scene, lock to last known position
			}

			if (!_isRopeInitialized) {
				InitializeRope(currentAnchor);
				_isRopeInitialized = true;
			}
			
			UpdateVerlet(delta);
		} else {
			anchorNode = null; // Reset tracking when rope is released/hidden
			ropeLine.Points = new Vector2[0];
			for(int i=0; i<=MaxSegments; i++) {
				positions[i] = Vector2.Zero;
				previousPositions[i] = Vector2.Zero;
			}
			_isRopeInitialized = false;
		}
	}

	void UpdateAim() {
		aimSprite.Visible = true;
		hookSprite.Visible = false;
		
		Vector2 armTip = ((player)player).GetArmTipPosition();
		Vector2 mousePos = GetGlobalMousePosition();
		Vector2 direction = (mousePos - armTip).Normalized();
		float dist = MaxLength; // Always aim/shoot to max distance
		
		// Small offset prevents tunneling into nearby walls; Exclude handles self-collision
		float offset = 10.0f;
		Vector2 origin = armTip + direction * offset;
		Vector2 end = origin + direction * (dist - offset);
		
		var spaceState = GetWorld2D().DirectSpaceState;
		var query = PhysicsRayQueryParameters2D.Create(origin, end);
		query.Exclude = new Godot.Collections.Array<Rid> { player.GetRid() }; // Completely ignores the player collider
		query.HitFromInside = true; // Fixes snapping to center/missing edges when ray starts near colliders
		var result = spaceState.IntersectRay(query);
		
		debugOrigin = origin;
		debugEnd = end;
		
		if (result.Count > 0) {
			debugHit = (Vector2)result["position"];
			hasHit = true;
			lastValidHit = debugHit;
			lastHitCollider = result.ContainsKey("collider") ? result["collider"].As<Node2D>() : null;
			if (lastHitCollider != null) {
				lastValidLocalHit = lastHitCollider.ToLocal(debugHit);
			}
			hasLastHit = true;
			aimSprite.GlobalPosition = debugHit;
		} else {
			hasHit = false;
			hasLastHit = false;
			lastHitCollider = null;
			aimSprite.GlobalPosition = end;
		}
		
		QueueRedraw();
	}

	public override void _Draw() {
		if (ropeState == RopeState.Hidden) {
			DrawLine(ToLocal(debugOrigin), ToLocal(debugEnd), Colors.Cyan, 2.0f);
			if (hasHit) {
				DrawCircle(ToLocal(debugHit), 6.0f, Colors.Red);
			} else {
				DrawCircle(ToLocal(debugEnd), 6.0f, Colors.Yellow);
			}
		} else if (ropeState == RopeState.Shot) {
			// Draw line from player to moving hook
			DrawLine(ToLocal(player.GlobalPosition), ToLocal(hookSprite.GlobalPosition), Colors.White, 2.0f);
		}
	}

	public override void _Input(InputEvent @event) {
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left) {
			if (mouseEvent.Pressed) {
				switch (ropeState) {
					case RopeState.Hidden:
						StartShot();
						break;
					case RopeState.Shot:
					case RopeState.Hooked:
					case RopeState.Slack:
					case RopeState.Retracting:
						CancelRope();
						break;
				}
			}
		} else if (@event is InputEventMouseButton rightMouse && rightMouse.ButtonIndex == MouseButton.Right) {
			if (rightMouse.Pressed && ropeState == RopeState.Hooked) {
				ropeState = RopeState.Retracting;
			} else if (!rightMouse.Pressed && ropeState == RopeState.Retracting) {
				ropeState = RopeState.Hooked;
			}
		}
		
		//GD.Print($"Rope State changed to: {ropeState}");
	}

	void StartShot() {
		if (ropeState != RopeState.Hidden) return;
		
		shotOrigin = ((player)player).GetArmTipPosition();
		shotDirection = (debugEnd - shotOrigin).Normalized();
		shotDistance = MaxLength; // Always shoot to max distance
		shotTraveled = 0f;
		isShooting = true;
		ropeState = RopeState.Shot;
		
		aimSprite.Visible = false;
		hookSprite.Visible = true;
		hookSprite.GlobalPosition = shotOrigin;
		
		shootSFX.Play();
	}

	public void CancelRope() {
		ropeState = RopeState.Hidden;
		isShooting = false;
		hookSprite.Visible = false;
		ropeLine.Points = new Vector2[0];
		for(int i=0; i<=MaxSegments; i++) {
			positions[i] = Vector2.Zero;
			previousPositions[i] = Vector2.Zero;
		}
		_isRopeInitialized = false;
		anchorNode = null;
	}

	void UpdateShot(double delta) {
		float dt = (float)delta;
		float moveStep = ShotSpeed * dt;
		
		hookSprite.GlobalPosition += shotDirection * moveStep;
		shotTraveled += moveStep;
		
		if (shotTraveled >= shotDistance) {
			if (hasLastHit) {
				// Successfully hit something, hook!
				anchorNode = lastHitCollider;
				if (anchorNode != null) {
					anchorLocalOffset = lastValidLocalHit;
					currentAnchor = anchorNode.ToGlobal(anchorLocalOffset);
				} else {
					currentAnchor = lastValidHit;
				}
				deployedLength = player.GlobalPosition.DistanceTo(currentAnchor);
				ropeState = RopeState.Hooked;
			} else {
				// Reached max distance without hitting anything, retract
				CancelRope();
			}
		}
		
		QueueRedraw();
	}

	void InitializeRope(Vector2 anchor) {
		Vector2 end = ((player)player).GetArmTipPosition();
		float dist = anchor.DistanceTo(end);
		SegmentLength = dist / MaxSegments;
		
		for (int i = 0; i <= MaxSegments; i++) {
			float t = i / (float)MaxSegments;
			Vector2 pos = anchor.Lerp(end, t);
			positions[i] = pos;
			previousPositions[i] = pos;
		}
		currentAnchor = anchor;
	}

	void UpdateVerlet(double delta) {
		float dt = Mathf.Clamp((float)delta, 0.0f, 0.033f);
		
		for (int i = 0; i <= MaxSegments; i++) {
			Vector2 vel = (positions[i] - previousPositions[i]) * 0.96f;
			previousPositions[i] = positions[i];
			
			if (i > 0) {
				positions[i] += vel + new Vector2(0, RopeGravity) * dt * dt;
			} else {
				positions[i] += vel;
			}
		}

		positions[0] = currentAnchor;
		previousPositions[0] = currentAnchor;

		Vector2 armTip = ((player)player).GetArmTipPosition();
		positions[MaxSegments] = armTip;
		previousPositions[MaxSegments] = armTip;

		int iterations = 15;
		for (int iter = 0; iter < iterations; iter++) {
			for (int i = 0; i < MaxSegments; i++) {
				Vector2 p1 = positions[i];
				Vector2 p2 = positions[i+1];
				Vector2 diff = p2 - p1;
				float dist = diff.Length();
				
				if (dist == 0) continue;
				
				float error = (dist - SegmentLength) / dist;
				Vector2 correction = diff * error * 0.5f;
				
				if (i == 0) {
					positions[i+1] -= correction;
				} else if (i + 1 == MaxSegments) {
					positions[i] += correction;
				} else {
					positions[i] += correction;
					positions[i+1] -= correction;
				}
			}
		}

		var localPoints = new Vector2[MaxSegments + 1];
		for(int i=0; i<=MaxSegments; i++) {
			localPoints[i] = ToLocal(positions[i]);
		}
		ropeLine.Points = localPoints;
	}
}
