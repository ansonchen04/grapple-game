using Godot;
using System;

/// <summary>
/// Manages the grapple rope, including shooting, hooking, and Verlet simulation.
/// </summary>
public partial class Rope : Node2D
{
	/// <summary>
	/// Reference to the player character.
	/// </summary>
	CharacterBody2D player;

	/// <summary>
	/// Current state of the rope.
	/// </summary>
	public RopeState ropeState;

	/// <summary>
	/// Maximum length of the rope.
	/// </summary>
	const float MaxLength = 500.0f;

	/// <summary>
	/// Positions of the rope segments.
	/// </summary>
	Vector2[] positions;
	Vector2[] previousPositions;

	/// <summary>
	/// Maximum number of segments in the rope.
	/// </summary>
	const int MaxSegments = 30;

	/// <summary>
	/// Visual components.
	/// </summary>
	Line2D ropeLine;
	Sprite2D hookSprite;
	Sprite2D aimSprite;
	AudioStreamPlayer shootSFX;

	/// <summary>
	/// Length of each segment.
	/// </summary>
	float SegmentLength;

	/// <summary>
	/// Gravity applied to the rope.
	/// </summary>
	const float RopeGravity = 200.0f;

	/// <summary>
	/// Flag indicating if the rope has been initialized.
	/// </summary>
	bool _isRopeInitialized = false;

	/// <summary>
	/// Current and previous anchor positions.
	/// </summary>
	Vector2 currentAnchor = Vector2.Zero;
	Vector2 previousAnchor = Vector2.Zero;

	/// <summary>
	/// Debug visualization variables.
	/// </summary>
	Vector2 debugOrigin = Vector2.Zero;
	Vector2 debugEnd = Vector2.Zero;
	Vector2 debugHit = Vector2.Zero;
	bool hasHit = false;

	/// <summary>
	/// Last valid hit information.
	/// </summary>
	Vector2 lastValidHit = Vector2.Zero;
	Vector2 lastValidLocalHit = Vector2.Zero;
	bool hasLastHit = false;

	/// <summary>
	/// Gets the current anchor position.
	/// </summary>
	public Vector2 GetAnchor() => currentAnchor;

	/// <summary>
	/// Gets the maximum rope length.
	/// </summary>
	public float GetMaxRopeLength() => deployedLength;

	/// <summary>
	/// Gets the hook's global position.
	/// </summary>
	public Vector2 GetHookPosition() => hookSprite.GlobalPosition;

	/// <summary>
	/// Current deployed length of the rope.
	/// </summary>
	float deployedLength = MaxLength;

	/// <summary>
	/// The node the rope is anchored to.
	/// </summary>
	Node2D anchorNode = null;

	/// <summary>
	/// Local offset of the anchor on the anchor node.
	/// </summary>
	Vector2 anchorLocalOffset = Vector2.Zero;

	/// <summary>
	/// The last collider hit by the hook.
	/// </summary>
	Node2D lastHitCollider = null;

	/// <summary>
	/// Last known global anchor position.
	/// </summary>
	Vector2 lastAnchorGlobal = Vector2.Zero;

	/// <summary>
	/// Shooting state variables.
	/// </summary>
	Vector2 shotOrigin = Vector2.Zero;
	Vector2 shotDirection = Vector2.Zero;
	float shotDistance = 0f;
	float shotTraveled = 0f;
	const float ShotSpeed = 1500.0f;
	bool isShooting = false;

	/// <summary>
	/// Called when the node enters the scene tree.
	/// </summary>
	public override void _Ready()
	{
		player = GetNode<CharacterBody2D>("../Player");
		ropeLine = GetNode<Line2D>("RopeLine");
		hookSprite = GetNode<Sprite2D>("HookSprite");
		aimSprite = GetNode<Sprite2D>("AimSprite");
		shootSFX = GetNode<AudioStreamPlayer>("ShootSFX");
		ropeState = RopeState.Hidden;

		positions = new Vector2[MaxSegments + 1];
		previousPositions = new Vector2[MaxSegments + 1];
	}

	/// <summary>
	/// Gets the player character.
	/// </summary>
	public CharacterBody2D GetPlayer() => player;

	/// <summary>
	/// Called every frame. Updates aim or shot state.
	/// </summary>
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

	/// <summary>
	/// Called every physics frame. Updates anchor tracking and Verlet simulation.
	/// </summary>
	public override void _PhysicsProcess(double delta)
	{
		if (ropeState == RopeState.Hooked || ropeState == RopeState.Retracting)
		{
			// Dynamic anchor tracking
			if (anchorNode != null && GodotObject.IsInstanceValid(anchorNode))
			{
				Vector2 newAnchor = anchorNode.GlobalPosition + anchorLocalOffset;

				if (previousAnchor == Vector2.Zero)
				{
					previousAnchor = currentAnchor;
				}

				// Smooth anchor transition
				float smoothFactor = 0.5f;
				currentAnchor = currentAnchor.Lerp(newAnchor, smoothFactor);
				previousAnchor = currentAnchor;
			}
			else
			{
				anchorNode = null;
			}

			if (!_isRopeInitialized)
			{
				InitializeRope(currentAnchor);
				_isRopeInitialized = true;
			}

			UpdateVerlet(delta);

			// Rotate hook
			Vector2 toAnchor = currentAnchor - hookSprite.GlobalPosition;
			if (toAnchor.Length() > 0.001f)
			{
				hookSprite.Rotation = Mathf.Atan2(toAnchor.Y, toAnchor.X);
			}
		}
		else
		{
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

	/// <summary>
	/// Updates the aim visualization and raycast.
	/// </summary>
	private void UpdateAim()
	{
		aimSprite.Visible = true;
		hookSprite.Visible = false;

		Vector2 armTip = player.GetArmTipPosition();
		Vector2 mousePos = GetGlobalMousePosition();
		Vector2 direction = (mousePos - armTip).Normalized();
		float dist = MaxLength;

		float offset = 10.0f;
		Vector2 origin = armTip + direction * offset;
		Vector2 end = origin + direction * (dist - offset);

		var spaceState = GetWorld2D().DirectSpaceState;
		var query = PhysicsRayQueryParameters2D.Create(origin, end);
		query.Exclude = new Godot.Collections.Array<Rid> { player.GetRid() };
		query.HitFromInside = true;
		var result = spaceState.IntersectRay(query);

		debugOrigin = origin;
		debugEnd = end;

		if (result.Count > 0)
		{
			debugHit = (Vector2)result["position"];
			hasHit = true;
			lastValidHit = debugHit;
			lastHitCollider = result.ContainsKey("collider") ? result["collider"].As<Node2D>() : null;
			if (lastHitCollider != null)
			{
				lastValidLocalHit = lastHitCollider.ToLocal(debugHit);
			}
			hasLastHit = true;
			aimSprite.GlobalPosition = debugHit;
		}
		else
		{
			hasHit = false;
			hasLastHit = false;
			lastHitCollider = null;
			aimSprite.GlobalPosition = end;
		}

		QueueRedraw();
	}

	/// <summary>
	/// Draws the aim line or shot line.
	/// </summary>
	public override void _Draw()
	{
		if (ropeState == RopeState.Hidden)
		{
			DrawLine(ToLocal(debugOrigin), ToLocal(debugEnd), Colors.Cyan, 2.0f);
			if (hasHit)
			{
				DrawCircle(ToLocal(debugHit), 6.0f, Colors.Red);
			}
			else
			{
				DrawCircle(ToLocal(debugEnd), 6.0f, Colors.Yellow);
			}
		}
		else if (ropeState == RopeState.Shot)
		{
			DrawLine(ToLocal(player.GlobalPosition), ToLocal(hookSprite.GlobalPosition), Colors.White, 2.0f);
		}
	}

	/// <summary>
	/// Handles input for shooting, canceling, and retracting the rope.
	/// </summary>
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

	/// <summary>
	/// Starts shooting the hook.
	/// </summary>
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

	/// <summary>
	/// Cancels the rope and resets state.
	/// </summary>
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

	/// <summary>
	/// Updates the shot animation and checks for collisions.
	/// </summary>
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
				CancelRope();
			}
		}

		QueueRedraw();
	}

	/// <summary>
	/// Initializes the rope segments between the anchor and the player.
	/// </summary>
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

	/// <summary>
	/// Updates the Verlet simulation for the rope.
	/// </summary>
	private void UpdateVerlet(double delta)
	{
		float dt = Mathf.Clamp((float)delta, 0.0f, 0.033f);

		for (int i = 0; i <= MaxSegments; i++)
		{
			Vector2 vel = (positions[i] - previousPositions[i]) * 0.96f;
			previousPositions[i] = positions[i];

			if (i > 0)
			{
				positions[i] += vel + new Vector2(0, RopeGravity) * dt * dt;
			}
			else
			{
				positions[i] += vel;
			}
		}

		positions[0] = currentAnchor;
		previousPositions[0] = currentAnchor;

		Vector2 armTip = player.GetArmTipPosition();
		positions[MaxSegments] = armTip;
		previousPositions[MaxSegments] = armTip;

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

				float error = (dist - SegmentLength) / dist;
				Vector2 correction = diff * error * 0.5f;

				if (i == 0)
				{
					positions[i + 1] -= correction;
				}
				else if (i + 1 == MaxSegments)
				{
					positions[i] += correction;
				}
				else
				{
					positions[i] += correction;
					positions[i + 1] -= correction;
				}
			}
		}

		var localPoints = new Vector2[MaxSegments + 1];
		for (int i = 0; i <= MaxSegments; i++)
		{
			localPoints[i] = ToLocal(positions[i]);
		}
		ropeLine.Points = localPoints;
	}
}
