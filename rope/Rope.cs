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
	float SegmentLength; 
	const float RopeGravity = 400.0f;
	bool _isRopeInitialized = false;
	Vector2 currentAnchor = Vector2.Zero;

	public override void _Ready() {
		player = GetNode<CharacterBody2D>("../Player");
		ropeLine = GetNode<Line2D>("RopeLine");
		hookSprite = GetNode<Sprite2D>("HookSprite");
		ropeState = RopeState.Hidden;
		
		positions = new Vector2[MaxSegments + 1];
		previousPositions = new Vector2[MaxSegments + 1];
	}

	public override void _Process(double delta) {
		if (ropeState == RopeState.Hidden || ropeState == RopeState.Shot) {
			UpdateAim();
		} else {
			hookSprite.Visible = false;
		}
	}

	public override void _PhysicsProcess(double delta) {
		if (ropeState == RopeState.Hooked) {
			if (!_isRopeInitialized) {
				InitializeRope(currentAnchor);
				_isRopeInitialized = true;
			}
			UpdateVerlet(delta);
		} else {
			ropeLine.Points = new Vector2[0];
			for(int i=0; i<=MaxSegments; i++) {
				positions[i] = Vector2.Zero;
				previousPositions[i] = Vector2.Zero;
			}
			_isRopeInitialized = false;
		}
	}

	void UpdateAim() {
		hookSprite.Visible = true;
		
		Vector2 mousePos = GetGlobalMousePosition();
		Vector2 direction = (mousePos - player.GlobalPosition).Normalized();
		float dist = Mathf.Min(player.GlobalPosition.DistanceTo(mousePos), MaxLength);
		
		// Offset origin slightly to prevent self-collision with player hitbox
		Vector2 origin = player.GlobalPosition + direction * 10.0f;
		Vector2 end = origin + direction * (dist - 10.0f);
		
		var spaceState = GetWorld2D().DirectSpaceState;
		var query = PhysicsRayQueryParameters2D.Create(origin, end);
		query.CollisionMask = uint.MaxValue & ~4u;
		var result = spaceState.IntersectRay(query);
		
		if (result.Count > 0) {
			hookSprite.GlobalPosition = (Vector2)result["position"];
		} else {
			hookSprite.GlobalPosition = end;
		}
	}

	public override void _Input(InputEvent @event) {
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left) {
			switch (ropeState) {
				case RopeState.Hidden:
					Vector2 direction = (mouseEvent.GlobalPosition - player.GlobalPosition).Normalized();
					float dist = Mathf.Min(player.GlobalPosition.DistanceTo(mouseEvent.GlobalPosition), MaxLength);
					
					// Offset origin slightly to prevent self-collision with player hitbox
					Vector2 origin = player.GlobalPosition + direction * 10.0f;
					Vector2 end = origin + direction * (dist - 10.0f);
					
					var spaceState = GetWorld2D().DirectSpaceState;
					var query = PhysicsRayQueryParameters2D.Create(origin, end);
					query.CollisionMask = uint.MaxValue & ~4u;
					var result = spaceState.IntersectRay(query);
					
					if (result.Count > 0) {
						currentAnchor = (Vector2)result["position"];
						ropeState = RopeState.Hooked;
					} else {
						ropeState = RopeState.Hidden;
					}
					break;
				case RopeState.Hooked:
				case RopeState.Slack:
					ropeState = RopeState.Hidden;
					break;
			}
			GD.Print($"Rope State changed to: {ropeState}");
		}
	}

	void InitializeRope(Vector2 anchor) {
		Vector2 end = player.GlobalPosition;
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

		positions[MaxSegments] = player.GlobalPosition;
		previousPositions[MaxSegments] = player.GlobalPosition;

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
