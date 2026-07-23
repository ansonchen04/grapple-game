using Godot;
using System;

// Manages the grapple rope, including shooting, hooking, and Verlet simulation.
// The rope is simulated as a chain of points (Verlet integration) for visual sag and flexibility.
// The player's physics are handled separately in player.cs using a spring-damper constraint.
public partial class Rope : Node2D
{
	// Reference to the player character.
	CharacterBody2D player;

	// Current state of the rope (Hidden, Shot, Hooked, Retracting, Slack).
	public RopeState ropeState;

	// Maximum length of the rope.
	const float MaxLength = 500.0f;

	// Positions of the rope segments for Verlet simulation.
	// positions[i] is the current position, previousPositions[i] is the position from the last frame.
	Vector2[] positions;
	Vector2[] previousPositions;

	// Maximum number of segments in the rope.
	const int MaxSegments = 30;

	// Visual components.
	Line2D ropeLine;
	Sprite2D hookSprite;
	Sprite2D aimSprite;
	AudioStreamPlayer shootSFX;

	// Length of each segment, calculated when the rope is initialized.
	float SegmentLength;

	// Gravity applied to the rope segments to create sag.
	const float RopeGravity = 200.0f;

	// Flag indicating if the rope has been initialized.
	bool _isRopeInitialized = false;

	// Current and previous anchor positions.
	// currentAnchor is the position the rope is attached to.
	// previousAnchor is used to smooth out anchor movement for moving platforms.
	Vector2 currentAnchor = Vector2.Zero;
	Vector2 previousAnchor = Vector2.Zero;

	// Debug visualization variables.
	Vector2 debugOrigin = Vector2.Zero;
	Vector2 debugEnd = Vector2.Zero;
	Vector2 debugHit = Vector2.Zero;
	bool hasHit = false;

	// Last valid hit information.
	Vector2 lastValidHit = Vector2.Zero;
	Vector2 lastValidLocalHit = Vector2.Zero;
	bool hasLastHit = false;

	// Gets the current anchor position.
	public Vector2 GetAnchor() => currentAnchor;

	// Gets the maximum rope length.
	public float GetMaxRopeLength() => deployedLength;

	// Gets the hook's global position.
	public Vector2 GetHookPosition() => hookSprite.GlobalPosition;

	// Current deployed length of the rope.
	float deployedLength = MaxLength;

	// The node the rope is anchored to.
	Node2D anchorNode = null;

	// Local offset of the anchor on the anchor node.
	// Used to track the anchor point on moving platforms.
	Vector2 anchorLocalOffset = Vector2.Zero;

	// The last collider hit by the hook.
	Node2D lastHitCollider = null;

	// Last known global anchor position.
	Vector2 lastAnchorGlobal = Vector2.Zero;

	// Shooting state variables.
	Vector2 shotOrigin = Vector2.Zero;
	Vector2 shotDirection = Vector2.Zero;
	float shotDistance = 0f;
	float shotTraveled = 0f;
	const float ShotSpeed = 1500.0f;
	bool isShooting = false;

	// Called when the node enters the scene tree.
	// Initializes nodes and arrays for Verlet simulation.
	public override void _Ready()
	{
		player = GetNode<CharacterBody2D>("../Player");
		ropeLine = GetNode<Line2D>("RopeLine");
		hookSprite = GetNode<Sprite2D>("HookSprite");
		aimSprite = GetNode<Sprite2D>("AimSprite");
		shootSFX = GetNode<AudioStreamPlayer>("ShootSFX");
		ropeState = RopeState.Hidden;

		// Allocate arrays for Verlet simulation.
		// MaxSegments + 1 because we have segments between points, so N segments need N+1 points.
		positions = new Vector2[MaxSegments + 1];
		previousPositions = new Vector2[MaxSegments + 1];
	}

	// Gets the player character.
	public CharacterBody2D GetPlayer() => player;

	// Called every frame. Updates aim or shot state.
	// _Process is used for visual updates and non-physics logic.
	public override void _Process(double delta)
	{
		if (ropeState == RopeState.Hidden)
		{
			UpdateAim();
		}
		else if (ropeState == RopeState.Shot)
		{
			UpdateShot(delta);
		}
		else
		{
			hookSprite.Visible = false;
			aimSprite.Visible = false;
		}
	}

	// Called every physics frame. Updates anchor tracking and Verlet simulation.
	// _PhysicsProcess is used for physics-related updates.
	public override void _PhysicsProcess(double delta)
	{
		if (ropeState == RopeState.Hooked || ropeState == RopeState.Retracting)
		{
			// Dynamic anchor tracking: update the anchor position if it's attached to a moving platform.
			if (anchorNode != null && GodotObject.IsInstanceValid(anchorNode))
			{
				// Calculate the new anchor position based on the platform's global position and the local offset.
				Vector2 newAnchor = anchorNode.GlobalPosition + anchorLocalOffset;

				// Initialize previousAnchor if it's the first frame.
				if (previousAnchor == Vector2.Zero)
				{
					previousAnchor = currentAnchor;
				}

				// Smooth anchor transition to prevent sudden jumps that cause Verlet instability.
				// Lerp towards the new anchor position with a smoothing factor.
				float smoothFactor = 0.5f;
				currentAnchor = currentAnchor.Lerp(newAnchor, smoothFactor);
				previousAnchor = currentAnchor;
			}
			else
			{
				// If the anchor node is invalid, clear it.
				anchorNode = null;
			}

			// Initialize the rope if it hasn't been initialized yet.
			if (!_isRopeInitialized)
			{
				InitializeRope(currentAnchor);
				_isRopeInitialized = true;
			}

			// Update the Verlet simulation for the rope.
			UpdateVerlet(delta);

			// Rotate the hook sprite to face the anchor.
			Vector2 toAnchor = currentAnchor - hookSprite.GlobalPosition;
			if (toAnchor.Length() > 0.001f)
			{
				hookSprite.Rotation = Mathf.Atan2(toAnchor.Y, toAnchor.X);
			}
		}
		else
		{
			// Reset the rope state if it's not hooked or retracting.
			anchorNode = null;
			previousAnchor = Vector2.Zero;
			ropeLine.Points = new Vector2[0];
			for (int i = 0; i <= MaxSegments; i++)
			{
				positions[i] = Vector2.Zero;
				previousPositions[i] = Vector2.Zero;
			}
			_isRopeInitialized = false;
		}
	}

	// Updates the aim visualization and raycast.
	// This method is called every frame when the rope is hidden.
	// It casts a ray from the player's arm tip towards the mouse cursor to find a potential anchor point.
	private void UpdateAim()
	{
		aimSprite.Visible = true;
		hookSprite.Visible = false;

		Vector2 armTip = player.GetArmTipPosition();
		Vector2 mousePos = GetGlobalMousePosition();
		Vector2 direction = (mousePos - armTip).Normalized();
		float dist = MaxLength;

		// Add a small offset to prevent the ray from hitting the player's own collider.
		float offset = 10.0f;
		Vector2 origin = armTip + direction * offset;
		Vector2 end = origin + direction * (dist - offset);

		// Perform a raycast to find the first collider in the path.
		var spaceState = GetWorld2D().DirectSpaceState;
		var query = PhysicsRayQueryParameters2D.Create(origin, end);
		query.Exclude = new Godot.Collections.Array<Rid> { player.GetRid() }; // Exclude the player from the raycast.
		query.HitFromInside = true; // Allow the ray to hit colliders even if it starts inside one.
		var result = spaceState.IntersectRay(query);

		debugOrigin = origin;
		debugEnd = end;

		if (result.Count > 0)
		{
			// If the ray hit something, store the hit position and collider.
			debugHit = (Vector2)result["position"];
			hasHit = true;
			lastValidHit = debugHit;
			lastHitCollider = result.ContainsKey("collider") ? result["collider"].As<Node2D>() : null;
			if (lastHitCollider != null)
			{
				// Store the local offset of the hit point on the collider.
				// This is used to track the anchor point on moving platforms.
				lastValidLocalHit = lastHitCollider.ToLocal(debugHit);
			}
			hasLastHit = true;
			aimSprite.GlobalPosition = debugHit;
		}
		else
		{
			// If the ray didn't hit anything, reset the hit state.
			hasHit = false;
			hasLastHit = false;
			lastHitCollider = null;
			aimSprite.GlobalPosition = end;
		}

		// Request a redraw to update the aim line.
		QueueRedraw();
	}

	// Draws the aim line or shot line.
	// This method is called by Godot to draw custom shapes.
	public override void _Draw()
	{
		if (ropeState == RopeState.Hidden)
		{
			// Draw the aim line from the arm tip to the mouse cursor or hit point.
			DrawLine(ToLocal(debugOrigin), ToLocal(debugEnd), Colors.Cyan, 2.0f);
			if (hasHit)
			{
				// Draw a red circle at the hit point.
				DrawCircle(ToLocal(debugHit), 6.0f, Colors.Red);
			}
			else
			{
				// Draw a yellow circle at the end of the aim line.
				DrawCircle(ToLocal(debugEnd), 6.0f, Colors.Yellow);
			}
		}
		else if (ropeState == RopeState.Shot)
		{
			// Draw the shot line from the player to the moving hook.
			DrawLine(ToLocal(player.GlobalPosition), ToLocal(hookSprite.GlobalPosition), Colors.White, 2.0f);
		}
	}

	// Handles input for shooting, canceling, and retracting the rope.
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left)
		{
			if (mouseEvent.Pressed)
			{
				switch (ropeState)
				{
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
		}
		else if (@event is InputEventMouseButton rightMouse && rightMouse.ButtonIndex == MouseButton.Right)
		{
			if (rightMouse.Pressed && ropeState == RopeState.Hooked)
			{
				ropeState = RopeState.Retracting;
			}
			else if (!rightMouse.Pressed && ropeState == RopeState.Retracting)
			{
				ropeState = RopeState.Hooked;
			}
		}
	}

	// Starts shooting the hook.
	private void StartShot()
	{
		if (ropeState != RopeState.Hidden) return;

		shotOrigin = player.GetArmTipPosition();
		shotDirection = (debugEnd - shotOrigin).Normalized();
		shotDistance = MaxLength;
		shotTraveled = 0f;
		isShooting = true;
		ropeState = RopeState.Shot;

		aimSprite.Visible = false;
		hookSprite.Visible = true;
		hookSprite.GlobalPosition = shotOrigin;

		shootSFX.Play();
	}

	// Cancels the rope and resets state.
	public void CancelRope()
	{
		ropeState = RopeState.Hidden;
		isShooting = false;
		hookSprite.Visible = false;
		ropeLine.Points = new Vector2[0];
		for (int i = 0; i <= MaxSegments; i++)
		{
			positions[i] = Vector2.Zero;
			previousPositions[i] = Vector2.Zero;
		}
		_isRopeInitialized = false;
		anchorNode = null;
	}

	// Updates the shot animation and checks for collisions.
	// This method is called every frame when the rope is in the "Shot" state.
	private void UpdateShot(double delta)
	{
		float dt = (float)delta;
		float moveStep = ShotSpeed * dt;

		hookSprite.GlobalPosition += shotDirection * moveStep;
		shotTraveled += moveStep;

		hookSprite.Rotation = Mathf.Atan2(shotDirection.Y, shotDirection.X);

		if (shotTraveled >= shotDistance)
		{
			if (hasLastHit)
			{
				// If the hook hit something, set the anchor point and switch to the "Hooked" state.
				anchorNode = lastHitCollider;
				if (anchorNode != null)
				{
					lastAnchorGlobal = lastValidHit;
					anchorLocalOffset = lastValidHit - anchorNode.GlobalPosition;
					currentAnchor = lastValidHit;
				}
				else
				{
					currentAnchor = lastValidHit;
				}
				deployedLength = player.GlobalPosition.DistanceTo(currentAnchor);
				ropeState = RopeState.Hooked;
			}
			else
			{
				// If the hook didn't hit anything, cancel the rope.
				CancelRope();
			}
		}

		QueueRedraw();
	}

	// Initializes the rope segments between the anchor and the player.
	// This method is called when the rope is first hooked.
	private void InitializeRope(Vector2 anchor)
	{
		Vector2 end = player.GetArmTipPosition();
		float dist = anchor.DistanceTo(end);
		SegmentLength = dist / MaxSegments;

		for (int i = 0; i <= MaxSegments; i++)
		{
			float t = i / (float)MaxSegments;
			Vector2 pos = anchor.Lerp(end, t);
			positions[i] = pos;
			previousPositions[i] = pos;
		}
		currentAnchor = anchor;
		previousAnchor = anchor;
	}

	// Updates the Verlet simulation for the rope.
	// Verlet integration is a numerical method for computing the motion of particles.
	// It's used here to simulate the rope's sag and flexibility.
	private void UpdateVerlet(double delta)
	{
		float dt = Mathf.Clamp((float)delta, 0.0f, 0.033f);

		// Update positions based on previous positions and velocity.
		for (int i = 0; i <= MaxSegments; i++)
		{
			Vector2 vel = (positions[i] - previousPositions[i]) * 0.96f; // 0.96 is a damping factor.
			previousPositions[i] = positions[i];

			if (i > 0)
			{
				// Apply gravity to all segments except the anchor.
				positions[i] += vel + new Vector2(0, RopeGravity) * dt * dt;
			}
			else
			{
				// The anchor point doesn't have gravity.
				positions[i] += vel;
			}
		}

		// Pin the first segment to the anchor.
		positions[0] = currentAnchor;
		previousPositions[0] = currentAnchor;

		// Pin the last segment to the player's arm tip.
		Vector2 armTip = player.GetArmTipPosition();
		positions[MaxSegments] = armTip;
		previousPositions[MaxSegments] = armTip;

		// Iterate to satisfy constraints (maintain segment lengths).
		int iterations = 15;
		for (int iter = 0; iter < iterations; iter++)
		{
			for (int i = 0; i < MaxSegments; i++)
			{
				Vector2 p1 = positions[i];
				Vector2 p2 = positions[i + 1];
				Vector2 diff = p2 - p1;
				float dist = diff.Length();

				if (dist == 0) continue;

				// Calculate the correction needed to maintain the segment length.
				float error = (dist - SegmentLength) / dist;
				Vector2 correction = diff * error * 0.5f;

				if (i == 0)
				{
					// The anchor point is fixed, so only move the next point.
					positions[i + 1] -= correction;
				}
				else if (i + 1 == MaxSegments)
				{
					// The player's arm tip is fixed, so only move the previous point.
					positions[i] += correction;
				}
				else
				{
					// Move both points to maintain the segment length.
					positions[i] += correction;
					positions[i + 1] -= correction;
				}
			}
		}

		// Convert global positions to local positions for the Line2D node.
		var localPoints = new Vector2[MaxSegments + 1];
		for (int i = 0; i <= MaxSegments; i++)
		{
			localPoints[i] = ToLocal(positions[i]);
		}
		ropeLine.Points = localPoints;
	}
}
