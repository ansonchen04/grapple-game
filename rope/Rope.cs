using Godot;
using System;

public partial class Rope : Node2D {
	CharacterBody2D player;
	public RopeState ropeState;

	const float MaxLength = 500.0f;
	
	// Placeholders for Verlet integration (Chunk 2)
	Vector2[] positions;
	Vector2[] previousPositions;
	const int MaxSegments = 30;

    Line2D ropeLine;
    const float SegmentLength = MaxLength / MaxSegments;
    const float RopeGravity = 400.0f;

	public override void _Ready() {
		player = GetNode<CharacterBody2D>("../Player");
        ropeLine = GetNode<Line2D>("RopeLine");
		ropeState = RopeState.Hidden;
		
		positions = new Vector2[MaxSegments + 1];
		previousPositions = new Vector2[MaxSegments + 1];
	}

    public override void _PhysicsProcess(double delta) {
        if (ropeState == RopeState.Shot || ropeState == RopeState.Hooked) {
            // Initialize test rope if not already set up
            if (positions[0] == Vector2.Zero) {
                InitializeTestRope();
            }
            UpdateVerlet(delta);
        } else {
            ropeLine.Points = new Godot.Collections.Array<Vector2>();
            for(int i=0; i<=MaxSegments; i++) {
                positions[i] = Vector2.Zero;
                previousPositions[i] = Vector2.Zero;
            }
        }
    }

    void InitializeTestRope() {
        // Hardcoded anchor 200px above player for testing Chunk 2
        Vector2 anchor = player.GlobalPosition + new Vector2(0, -200);
        Vector2 end = player.GlobalPosition;
        
        for (int i = 0; i <= MaxSegments; i++) {
            float t = i / (float)MaxSegments;
            Vector2 pos = anchor.Lerp(end, t);
            positions[i] = pos;
            previousPositions[i] = pos;
        }
    }

    void UpdateVerlet(double delta) {
        float dt = (float)delta;
        
        // 1. Integration step: newPos = 2*current - previous + acc*dt^2
        for (int i = 0; i <= MaxSegments; i++) {
            Vector2 vel = positions[i] - previousPositions[i];
            previousPositions[i] = positions[i];
            
            if (i > 0) {
                positions[i] += vel + new Vector2(0, RopeGravity) * dt * dt;
            } else {
                positions[i] += vel;
            }
        }

        // Fix anchor point (index 0) to stay above player
        Vector2 anchor = player.GlobalPosition + new Vector2(0, -200);
        positions[0] = anchor;
        previousPositions[0] = anchor;

        // 2. Constraint relaxation: enforce fixed distance between points
        int iterations = 5;
        for (int iter = 0; iter < iterations; iter++) {
            for (int i = 0; i < MaxSegments; i++) {
                Vector2 p1 = positions[i];
                Vector2 p2 = positions[i+1];
                Vector2 diff = p2 - p1;
                float dist = diff.Length();
                
                if (dist == 0) continue;
                
                float error = (dist - SegmentLength) / dist;
                Vector2 correction = diff * error * 0.5f;
                
                // Don't move anchor or player attachment yet (handled later)
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

        // Update visuals
        var linePoints = new Godot.Collections.Array<Vector2>();
        for(int i=0; i<=MaxSegments; i++) linePoints.Add(positions[i]);
        ropeLine.Points = linePoints;
    }

	public override void _Input(InputEvent @event) {
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left) {
			switch (ropeState) {
				case RopeState.Hidden:
					ropeState = RopeState.Shot;
					break;
				case RopeState.Shot:
				case RopeState.Hooked:
				case RopeState.Slack:
					ropeState = RopeState.Hidden;
					break;
			}
			GD.Print($"Rope State changed to: {ropeState}");
		}
	}
}
