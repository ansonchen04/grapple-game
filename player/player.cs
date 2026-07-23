using Godot;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;

/// <summary>
/// The main player character controller. Handles movement, jumping, climbing, and grapple/swing physics.
/// </summary>
public partial class player : CharacterBody2D
{
	/// <summary>
	/// Base movement speed on the ground.
	/// </summary>
	private const float speed = 450.0f;
	/// <summary>
	/// Initial velocity applied when jumping.
	/// </summary>
	private const float jumpVelocity = -700.0f;

	/// <summary>
	/// Velocity applied when climbing.
	/// </summary>
	private const float climbVelocity = -200.0f;

	/// <summary>
	/// Dead zone for rope constraint to allow horizontal drift and match visual sag.
	/// </summary>
	private const float slackBuffer = 40.0f;

	/// <summary>
	/// Tunable friction for swinging against surfaces (0.0-0.3 recommended).
	/// </summary>
	public float swingFriction = 0.01f;

	/// <summary>
	/// The player's starting position in the current level.
	/// </summary>
	private Vector2 startPosition;

	/// <summary>
	/// The position of the last checkpoint the player touched.
	/// </summary>
	public static Vector2 LastCheckpointPosition { get; set; }

	/// <summary>
	/// The file path of the current level.
	/// </summary>
	public static string CurrentLevelPath { get; set; } = "res://level/level_1/level1.tscn";

	/// <summary>
	/// The Y-coordinate threshold for falling out of bounds.
	/// </summary>
	private float outOfBounds;

	/// <summary>
	/// The initial direction of the hook shot.
	/// </summary>
	private Vector2 hookStartPos;

	/// <summary>
	/// Gravity value from project settings.
	/// </summary>
	private float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	/// <summary>
	/// Raycast pointing downward to detect platforms.
	/// </summary>
	private RayCast2D _downwardRaycast;

	/// <summary>
	/// Length of the grapple raycast.
	/// </summary>
	private const float raycastLength = 500.0f;

	/// <summary>
	/// Raycast used for grapple aiming.
	/// </summary>
	private RayCast2D rayCast;

	/// <summary>
	/// Flag indicating if the player is currently grappled.
	/// </summary>
	private bool isGrappled = false;

	/// <summary>
	 /// Reference to the Rope node.
	/// </summary>
	public Rope rope;

	/// <summary>
	/// Audio players for various sound effects.
	/// </summary>
	private AudioStreamPlayer jumpSFX;
	private AudioStreamPlayer stepSFX;
	private AudioStreamPlayer landSFX;
	private AudioStreamPlayer dieSFX;

	/// <summary>
	/// Visual components for the player.
	/// </summary>
	private Sprite2D playerSprite;
	private Sprite2D monkeArm;
	private Texture2D swingTexture;

	/// <summary>
	/// Timer for playing step sounds.
	/// </summary>
	private float stepTimer = 0f;

	/// <summary>
	/// Distance from player center to the arm tip.
	/// </summary>
	public const float ArmTipOffset = 40.0f;

	/// <summary>
	/// Interval in seconds between step sounds.
	/// </summary>
	private const float stepInterval = 0.3f;

	/// <summary>
	/// Tracks if the player was on the floor in the previous frame.
	/// </summary>
	private bool wasOnFloor = false;

	/// <summary>
	/// Previous anchor position for velocity compensation.
	/// </summary>
	private Vector2 previousAnchor = Vector2.Zero;

	/// <summary>
	/// Smoothed anchor velocity to prevent spikes.
	/// </summary>
	private Vector2 smoothedAnchorVel = Vector2.Zero;

	/// <summary>
	/// ShapeCast2D for detecting surface friction while swinging.
	/// </summary>
	private ShapeCast2D frictionCast;

	/// <summary>
	/// Flags for special surface interactions.
	/// </summary>
	private bool onClimbableSurface = false;
	private bool onOneWaySurface = false;
	/// <summary>
	/// Called when the node enters the scene tree for the first time.
	/// Initializes nodes, variables, and event listeners.
	/// </summary>
	public override void _Ready()
	{
		// Load out-of-bounds threshold
		WorldBoundaryShape2D worldBoundary = GD.Load<WorldBoundaryShape2D>("res://template/outofbounds.tres");
		outOfBounds = worldBoundary.Distance;

		// Initialize nodes
		_downwardRaycast = GetNode<RayCast2D>("DownwardRaycast");
		rayCast = GetNode<RayCast2D>("RayCast2D");
		rayCast.Enabled = true;
		rayCast.CollisionMask = 1; // Target platform layer
		_downwardRaycast.CollisionMask = 1;

		rope = GetNode<Rope>("../Rope");
		jumpSFX = GetNode<AudioStreamPlayer>("JumpSFX");
		stepSFX = GetNode<AudioStreamPlayer>("StepSFX");
		landSFX = GetNode<AudioStreamPlayer>("LandSFX");
		dieSFX = GetNode<AudioStreamPlayer>("DieSFX");
		playerSprite = GetNode<Sprite2D>("Sprite2D");
		monkeArm = GetNode<Sprite2D>("MonkeArm");

		// Load textures
		swingTexture = GD.Load<Texture2D>("res://sprites/player/swing_body.png");
		playerSprite.Texture = swingTexture;

		// Initialize friction cast
		frictionCast = new ShapeCast2D();
		frictionCast.Shape = GetNode<CollisionShape2D>("CollisionShape2D").Shape;
		frictionCast.CollisionMask = CollisionMask;
		AddChild(frictionCast);

		// Set initial positions and paths
		startPosition = this.GlobalPosition;
		LastCheckpointPosition = startPosition;
		CurrentLevelPath = GetTree().CurrentScene.SceneFilePath;

		GD.Print($"Player: Loaded level {CurrentLevelPath}");
		GD.Print(startPosition);
	}
	/// <summary>
	/// Called every physics frame. Handles movement, gravity, swinging, and visual updates.
	/// </summary>
	public override void _PhysicsProcess(double delta)
	{
		// Check for out-of-bounds or restart
		if (Position.Y > outOfBounds || Input.IsActionJustPressed("Restart"))
		{
			restart();
			return;
		}

		// Check platform state
		if (_downwardRaycast.IsColliding())
		{
			var collider = _downwardRaycast.GetCollider();
			if (collider is Node2D platform)
			{
				setOneWay(checkOneway(platform));
			}
		}

		Vector2 newVelocity = Velocity;
		float dt = (float)delta;
		bool isSwinging = rope.ropeState == RopeState.Hooked || rope.ropeState == RopeState.Retracting;

		// 1. Gravity
		if (!IsOnFloor() && !onClimbableSurface && !isSwinging)
		{
			newVelocity.Y += gravity * dt * 1.5f;
		}

		// 2. Base Input Handling
		if (onClimbableSurface)
		{
			newVelocity = climbMovement(newVelocity);
		}
		else if (!isSwinging && onOneWaySurface && IsOnFloor() && Input.IsActionJustPressed("Down"))
		{
			Position += new Vector2(0, 5); // Drop through one-way platform
			newVelocity.Y = 100.0f;
		}
		else if (!isSwinging || IsOnFloor())
		{
			newVelocity = baseMovement(newVelocity, dt);
		}

		// 3. Rope Constraints & Tangential Input
		if (isSwinging)
		{
			ApplySwingPhysics(ref newVelocity, delta);
		}
		else
		{
			previousAnchor = Vector2.Zero;
		}

		// 4. Jump Override
		if (Input.IsActionJustPressed("Up") && IsOnFloor())
		{
			newVelocity.Y = jumpVelocity;
			jumpSFX.Play();
		}

		// 5. Walking SFX
		if (IsOnFloor() && Math.Abs(newVelocity.X) > 10.0f)
		{
			stepTimer += (float)delta;
			if (stepTimer >= stepInterval)
			{
				stepSFX.Play();
				stepTimer = 0f;
			}
		}
		else
		{
			stepTimer = 0f;
		}

		// 6. Landing SFX
		if (IsOnFloor() && !wasOnFloor)
		{
			landSFX.Play();
		}
		wasOnFloor = IsOnFloor();

		// 7. Flip sprite
		float inputX = Input.GetActionStrength("Right") - Input.GetActionStrength("Left");
		if (inputX != 0)
		{
			playerSprite.FlipH = inputX < 0;
		}

		// 8. Update monke arm
		monkeArm.Visible = true;
		monkeArm.GlobalPosition = GlobalPosition;

		float rotationOffset = Mathf.Pi / 2.0f;

		if (rope.ropeState == RopeState.Hidden)
		{
			Vector2 mousePos = GetGlobalMousePosition();
			Vector2 direction = mousePos - GlobalPosition;
			monkeArm.Rotation = Mathf.Atan2(direction.Y, direction.X) + rotationOffset;
		}
		else if (rope.ropeState == RopeState.Shot)
		{
			Vector2 hookPos = rope.GetHookPosition();
			Vector2 direction = hookPos - GlobalPosition;
			monkeArm.Rotation = Mathf.Atan2(direction.Y, direction.X) + rotationOffset;
		}
		else if (rope.ropeState == RopeState.Hooked || rope.ropeState == RopeState.Retracting)
		{
			Vector2 anchor = rope.GetAnchor();
			Vector2 direction = anchor - GlobalPosition;
			monkeArm.Rotation = Mathf.Atan2(direction.Y, direction.X) + rotationOffset;
		}

		Velocity = newVelocity;
		MoveAndSlide();
	}
	/// <summary>
	/// Handles input events, specifically grapple aiming.
	/// </summary>
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
		{
			Vector2 mousePosition = GetGlobalMousePosition();
			Vector2 direction = (mousePosition - GlobalPosition).Normalized();
			direction *= raycastLength;

			hookStartPos = direction;
			rayCast.TargetPosition = direction;
			rayCast.ForceRaycastUpdate();
		}
	}
	/// <summary>
	/// Handles basic movement on the ground and in the air.
	/// </summary>
	private Vector2 baseMovement(Vector2 velocity, float dt)
	{
		Vector2 direction = Input.GetVector("Left", "Right", "Up", "Down");

		if (direction.X != 0)
		{
			if (IsOnFloor())
			{
				// Grounded: snappy direct control
				velocity.X = Mathf.MoveToward(velocity.X, direction.X * speed, speed);
			}
			else
			{
				// Airborne: acceleration-based
				float airAccel = speed * 1.0f;
				velocity.X += direction.X * airAccel * dt;

				// Soft cap
				float maxAirSpeed = speed * 3.0f;
				velocity.X = Mathf.Clamp(velocity.X, -maxAirSpeed, maxAirSpeed);
			}
		}
		else
		{
			if (!IsOnFloor())
			{
				// Preserve aerial momentum
			}
			else
			{
				velocity.X = Mathf.MoveToward(velocity.X, 0, speed);
			}
		}
		return velocity;
	}
	/// <summary>
	/// Handles movement while climbing.
	/// </summary>
	private Vector2 climbMovement(Vector2 velocity)
	{
		Vector2 direction = Input.GetVector("Left", "Right", "Up", "Down");
		if (direction != Vector2.Zero)
		{
			velocity.X = direction.X * speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, speed);
		}

		if (Input.IsActionPressed("Up"))
			velocity.Y = climbVelocity;
		else if (Input.IsActionPressed("Down"))
			velocity.Y = -climbVelocity;
		else
			velocity.Y = 0;

		return velocity;
	}
	/// <summary>
	/// Checks if the platform is a one-way platform.
	/// </summary>
	private bool checkOneway(Node2D platform)
	{
		Node[] children = platform.FindChildren("*", "CollisionPolygon2D", false, false).ToArray();
		if (children.Length == 0)
		{
			children = platform.FindChildren("*", "CollisionShape2D", true, false).ToArray();
			if (children.Length > 0)
			{
				CollisionShape2D collisionShape = (CollisionShape2D)children[0];
				if (collisionShape != null && collisionShape.OneWayCollision)
				{
					return true;
				}
			}
		}
		else
		{
			CollisionPolygon2D collisionPolygon = (CollisionPolygon2D)children[0];
			if (collisionPolygon != null && collisionPolygon.OneWayCollision)
			{
				return true;
			}
		}
		return false;
	}
	/// <summary>
	/// Restarts the player at the last checkpoint.
	/// </summary>
	public void restart()
	{
		dieSFX.Play();
		Position = LastCheckpointPosition;
		Velocity = Vector2.Zero;
	}

	/// <summary>
	/// Sets the climbing state.
	/// </summary>
	public void setClimbing(bool onClimbableSurface)
	{
		this.onClimbableSurface = onClimbableSurface;
	}

	/// <summary>
	/// Sets the one-way surface state.
	/// </summary>
	public void setOneWay(bool onOneWaySurface)
	{
		this.onOneWaySurface = onOneWaySurface;
	}

	/// <summary>
	/// Gets the global position of the raycast.
	/// </summary>
	public Vector2 GetRaycastPos()
	{
		return rayCast.GlobalPosition;
	}

	/// <summary>
	/// Gets the start position of the hook.
	/// </summary>
	public Vector2 GetHookStartPos()
	{
		return hookStartPos + GlobalPosition;
	}

	/// <summary>
	/// Gets the position of the arm tip.
	/// </summary>
	public Vector2 GetArmTipPosition()
	{
		if (monkeArm == null) return GlobalPosition;
		float visualRotation = monkeArm.Rotation - Mathf.Pi / 2.0f;
		Vector2 direction = new Vector2(Mathf.Cos(visualRotation), Mathf.Sin(visualRotation));
		return monkeArm.GlobalPosition + direction * ArmTipOffset;
	}

	/// <summary>
	/// Gets the surface friction coefficient.
	/// </summary>
	private float GetSurfaceFriction(Node collider)
	{
		return swingFriction;
	}

	/// <summary>
	/// Applies swing physics constraints and forces.
	/// </summary>
	private void ApplySwingPhysics(ref Vector2 vel, double delta)
	{
		Vector2 anchor = rope.GetAnchor();
		float maxLen = rope.GetMaxRopeLength();

		if (previousAnchor == Vector2.Zero) previousAnchor = anchor;
		Vector2 anchorVel = (anchor - previousAnchor) / (float)delta;
		previousAnchor = anchor;

		// Smooth anchor velocity
		smoothedAnchorVel = smoothedAnchorVel * 0.8f + anchorVel * 0.2f;

		// Clamp anchor velocity
		if (smoothedAnchorVel.Length() > 1000.0f)
		{
			smoothedAnchorVel = smoothedAnchorVel.Normalized() * 1000.0f;
		}

		Vector2 anchorVelComp = smoothedAnchorVel;
		Vector2 toPlayer = GlobalPosition - anchor;
		float dist = toPlayer.Length();

		// Gravity
		vel.Y += gravity * (float)delta;

		bool isRetracting = rope.ropeState == RopeState.Retracting;

		if (dist > 0.001f)
		{
			Vector2 radialDir = toPlayer / dist;
			Vector2 tangentDir = new Vector2(radialDir.Y, -radialDir.X);

			// Relative velocity
			Vector2 relVel = vel - anchorVelComp;

			float radialSpeed = relVel.Dot(radialDir);
			float tangentialSpeed = relVel.Dot(tangentDir);
			Vector2 radialVel = radialSpeed * radialDir;
			Vector2 tangentialVel = tangentialSpeed * tangentDir;

			if (isRetracting)
			{
				// Retracting: Direct inward pull
				float retractForce = 1500.0f;
				relVel -= radialVel;
				relVel += tangentialVel;
				relVel -= radialDir * retractForce * (float)delta;
			}
			else
			{
				// Hooked: Spring/Damping Constraint
				float effectiveMaxLen = maxLen + slackBuffer;
				if (dist > effectiveMaxLen && radialSpeed > 0)
				{
					Vector2 radialVelCurrent = radialSpeed * radialDir;

					float blendStart = effectiveMaxLen;
					float blendEnd = effectiveMaxLen * 1.2f;
					float blendFactor = Mathf.Clamp((dist - blendStart) / (blendEnd - blendStart), 0.0f, 1.0f);

					float k = 350.0f;
					if (dist > effectiveMaxLen * 1.3f)
					{
						k *= Mathf.Clamp((dist - effectiveMaxLen * 1.3f) / (effectiveMaxLen * 0.2f), 1.0f, 5.0f);
					}
					float c = 2.0f * Mathf.Sqrt(k);
					float stretch = dist - maxLen;

					Vector2 springForce = (-k * stretch) * radialDir;
					Vector2 dampingForce = -c * radialVelCurrent;

					relVel += (springForce + dampingForce) * blendFactor * (float)delta;

					// Remove radial velocity if moving outward
					if (radialSpeed > 0)
					{
						relVel -= radialVelCurrent;
					}
				}
			}

			// Tangential Input
			if (dist > 0.1f && !IsOnFloor())
			{
				float inputX = Input.GetActionStrength("Right") - Input.GetActionStrength("Left");
				Vector2 inputDir = new Vector2(inputX, 0);
				Vector2 tangentialInput = inputDir - (inputDir.Dot(radialDir) * radialDir);

				if (relVel.Y > 0)
				{
					tangentialInput.Y *= 0.4f;
				}

				relVel += tangentialInput * speed * (float)delta * 2.0f;
			}

			// Convert back to absolute velocity
			vel = relVel + anchorVelComp;

			// Surface Friction
			if (frictionCast.IsColliding())
			{
				Node collider = frictionCast.GetCollider(0) as Node;
				if (collider != null && collider != this)
				{
					Vector2 normal = frictionCast.GetCollisionNormal(0);
					float frictionCoeff = GetSurfaceFriction(collider);

					Vector2 vPerp = normal * vel.Dot(normal);
					Vector2 vParallel = vel - vPerp;

					if (vParallel.Length() > 10.0f)
					{
						float dampFactor = Math.Max(0.0f, 1.0f - frictionCoeff * (float)delta * 60.0f);
						vParallel *= dampFactor;
					}
					vel = vPerp + vParallel;
				}
			}
		}
	}
}
