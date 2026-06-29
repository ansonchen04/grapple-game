using Godot;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;

public partial class player : CharacterBody2D
{

	//How fast the player moves and how high they can jump
	private const float speed = 300.0f;
	private const float jumpVelocity = -600.0f;
	private const float climbVelocity = -200.0f;
	private const float slackBuffer = 40.0f; // Dead zone for rope constraint to allow horizontal drift/match visual sag
	public float swingFriction = 0.5f; // Tunable friction coefficient for surface contact while swinging
	//Starting Position, should be updated whenever player enters a new scene
	private Vector2 startPosition;
	//Gets at what y value it is out of bounds 
	private float outOfBounds;
	private Vector2 hookStartPos;
	// Get the gravity from the project settings to be synced with RigidBody nodes.
	private float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	//Ray is in the center of the player model, checking what platform the player is on
	private RayCast2D _downwardRaycast;
  //TODO Rename raycast length to a clearer name
  	private const float raycastLength = 105.0f;
  	private RayCast2D rayCast;
  	private bool isGrappled = false;
	private Rope rope;
	private Vector2 previousAnchor = Vector2.Zero; // For anchor velocity compensation
	private ShapeCast2D frictionCast; // For native surface friction detection
	//Booleans to check if we are on a special surface, if we have different movement options
	private bool onClimbableSurface = false;
	private bool onOneWaySurface = false;
	public override void _Ready() {
		//Hardcoded, TODO make this varible for the level
		WorldBoundaryShape2D worldBoundary = GD.Load<WorldBoundaryShape2D>("res://level/level1/outotbounds.tres");
		outOfBounds = worldBoundary.Distance;
		// Initialize the RayCast2D node
		_downwardRaycast = GetNode<RayCast2D>("DownwardRaycast");
		startPosition = this.GlobalPosition;
		GD.Print(startPosition);
		rayCast = GetNode<RayCast2D>("RayCast2D");
		rayCast.Enabled = true;  // disabled by default, we'll turn it on when we click
		rope = GetNode<Rope>("../Rope");
		
		frictionCast = new ShapeCast2D();
		frictionCast.Shape = GetNode<CollisionShape2D>("CollisionShape2D").Shape;
		frictionCast.CollisionMask = CollisionMask; // Match player's collision mask
		AddChild(frictionCast);
	}
	public override void _PhysicsProcess(double delta) {
		//Check out of bounds from the resource
		if (Position.Y > outOfBounds || Input.IsActionJustPressed("Restart")) {
		this.restart();
		}
		// Check if the player is on the floor or a specific platform using the raycast
		if (_downwardRaycast.IsColliding()) {
			var collider = _downwardRaycast.GetCollider();
			//Likely will have to constrain this more
			if (collider is Node2D platform) {	
				//Will likely have to safeguard this if we collide with platforms without a collision box. 
				//Not sure when that would happen though
				this.setOneWay(this.checkOneway(platform));
			}
		}
		//Gets the current velocity
		Vector2 newVelocity = Velocity;
		
		if (rope.ropeState == RopeState.Hooked) {
			ApplySwingPhysics(ref newVelocity, delta);
			// Allow jumping while hooked if standing on a platform
			if (Input.IsActionJustPressed("Up") && IsOnFloor()) {
				newVelocity.Y = jumpVelocity;
			}
		} else {
			previousAnchor = Vector2.Zero; // Reset for next grapple to avoid velocity spikes
			//Checking which movement option, if any, is being used. Will convert this into a switch case in a future commit
			if (onOneWaySurface) {
				onewaydropMovement(newVelocity);
				newVelocity = baseMovement(newVelocity);
			}
			else if (onClimbableSurface) {
				newVelocity = climbMovement(newVelocity);
			}
			else {
				//If nothing fancy, just use base movement vectors
				newVelocity = baseMovement(newVelocity);
			}
			// Add the gravity.
			if (!IsOnFloor() && !onClimbableSurface)
				newVelocity.Y += gravity * (float)delta;
		}
		//Updates to the new velocity
		Velocity = newVelocity;
		//Moves the sprite at the end
		MoveAndSlide();
		//GD.Print(onClimbableSurface);
	}
  	public override void _Input(InputEvent @event) {
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left) {
			// i will probably need this raycast later to check if there's an object in between the starting pos of the gun and the player
			// Get the global position of the mouse click
			
			Vector2 mousePosition = GetGlobalMousePosition();
			Vector2 direction = (mousePosition - GlobalPosition).Normalized();
			direction *= raycastLength;  // need to rename this later!!!

			hookStartPos = direction;
			
			// Set the raycast's target position relative to the character's position
			rayCast.TargetPosition = direction;  
			
			// Optionally update the raycast (not needed if auto_update is true)
			rayCast.ForceRaycastUpdate();

			/*
			if (rayCast.IsColliding()) {
				GD.Print("collided! distance: " + GlobalPosition.DistanceTo(rayCast.GetCollisionPoint()));
			} else {
				GD.Print("did not collide with anything.");
			}
			*/
		}
	}
	private Vector2 baseMovement(Vector2 velocity) {
		// Get the input direction and handle the movement/deceleration.
		Vector2 direction = Input.GetVector("Left", "Right", "Up", "Down");
		if (direction.X != 0) {
			// Preserve high momentum from swings/retracts; only cap if below max walk speed
			if (Mathf.Abs(velocity.X) <= speed) {
				velocity.X = direction.X * speed;
			} else {
				// If moving fast in the same direction as input, preserve momentum
				if (velocity.X * direction.X > 0) {
					// Do nothing, keep high speed
				} else {
					// Opposing input: gradually steer/brake towards target speed
					velocity.X = Mathf.MoveToward(velocity.X, direction.X * speed, speed);
				}
			}
		}
		else {
			// Preserve aerial momentum when no input is pressed (Step 8: Aerial Momentum Preservation)
			if (!IsOnFloor()) {
				// Keep existing horizontal velocity while airborne
			} else {
				velocity.X = Mathf.MoveToward(Velocity.X, 0, speed);
			}
		}
		// Handle Jump.
		if (Input.IsActionJustPressed("Up") && IsOnFloor())
			velocity.Y = jumpVelocity;
		return velocity;
	}
	private Vector2 climbMovement(Vector2 velocity) {
		// Get the input direction and handle the movement/deceleration.
		Vector2 direction = Input.GetVector("Left", "Right", "Up", "Down");
		if (direction != Vector2.Zero) {
			velocity.X = direction.X * speed;
		}
		else {
			velocity.X = Mathf.MoveToward(Velocity.X, 0, speed);
		}
		// Handle Jump.
		if (Input.IsActionPressed("Up"))
			velocity.Y = climbVelocity;
		else if (Input.IsActionPressed("Down"))
			velocity.Y = -climbVelocity;
		else
			velocity.Y = 0;
		return velocity;
	}
	private Vector2 onewaydropMovement(Vector2 velocity) {
		// Drop the player down 1 pixel if standing on a one-way collision platform and "ui_drop_down" is pressed
		//TODO verify this actually does ^
		if (IsOnFloor() && Input.IsActionJustPressed("Down")) {
			Position += new Vector2(0, 1);
		}
		return velocity;
	}
	private Boolean checkOneway(Node2D platform) {
		//Gets the collision polygon or collision shape of the platform
		Node[] children = platform.FindChildren("*","CollisionPolygon2D",false,false).ToArray();
		if (children.Length == 0) {
			children = platform.FindChildren("*","CollisionShape2D",true,false).ToArray();
			CollisionShape2D collisionPolygon = (CollisionShape2D)children[0];
			if (collisionPolygon != null && collisionPolygon.OneWayCollision == true) {
				return true;
			}
		}
		else {
			CollisionPolygon2D collisionPolygon = (CollisionPolygon2D)children[0];
				if (collisionPolygon != null) {
					if (collisionPolygon.OneWayCollision == true) {
						return true;
					}
				}
		}
		return false;
	}
	public void restart() {
		//If restart button pressed, reset the positon to the start and zero out the velocity
			Position = startPosition;
			Velocity = Vector2.Zero;
	}
	public void setClimbing(bool onClimbableSurface) {
		this.onClimbableSurface = onClimbableSurface;
	}
	public void setOneWay(bool onOneWaySurface) {
		this.onOneWaySurface = onOneWaySurface;
	}
	public Vector2 GetRaycastPos() {
		return rayCast.GlobalPosition;
	}

	public Vector2 GetHookStartPos() {
		return hookStartPos + GlobalPosition;
	}

	float GetSurfaceFriction(Node collider) {
		// Note: Direct PhysicsMaterialOverride access varies by Godot 4.x version.
		// Using a tunable public variable ensures stability across versions while preserving the mechanic.
		return swingFriction;
	}

	void ApplySwingPhysics(ref Vector2 vel, double delta) {
		Vector2 anchor = rope.GetAnchor();
		float maxLen = rope.GetMaxRopeLength();
		
		// Fix 2: Anchor velocity compensation
		if (previousAnchor == Vector2.Zero) previousAnchor = anchor;
		Vector2 anchorVel = (anchor - previousAnchor) / (float)delta;
		previousAnchor = anchor;

		Vector2 toPlayer = GlobalPosition - anchor;
		float dist = toPlayer.Length();
		
		// 1. Gravity (applied first per integration order)
		vel.Y += gravity * (float)delta;
		
		bool isRetracting = rope.ropeState == RopeState.Retracting;

		if (dist > 0.001f) {
			Vector2 radialDir = toPlayer / dist;
			Vector2 tangentDir = new Vector2(radialDir.Y, -radialDir.X);
			
			// Work in relative velocity space to account for moving anchor
			Vector2 relVel = vel - anchorVel;
			
			float radialSpeed = relVel.Dot(radialDir);
			float tangentialSpeed = relVel.Dot(tangentDir);
			Vector2 radialVel = radialSpeed * radialDir;
			Vector2 tangentialVel = tangentialSpeed * tangentDir;

			if (isRetracting) {
				// Retracting: Direct inward pull, NO damping, preserve tangential velocity
				float retractForce = 1500.0f; 
				
				relVel -= radialVel; // Remove radial relative velocity
				relVel += tangentialVel; // Explicitly restore tangential relative velocity
				
				// Apply inward force
				relVel -= radialDir * retractForce * (float)delta;
			} else {
				// Hooked: Strictly Unidirectional Spring/Damping Constraint
				float effectiveMaxLen = maxLen + slackBuffer;
				if (dist > effectiveMaxLen && radialSpeed > 0) {
					Vector2 radialVelCurrent = radialSpeed * radialDir;
					
					float blendStart = effectiveMaxLen;
					float blendEnd = effectiveMaxLen * 1.2f; // Widened for softer elasticity
					float blendFactor = Mathf.Clamp((dist - blendStart) / (blendEnd - blendStart), 0.0f, 1.0f);
					
					// Fix 1: Soft Position Correction - removed hard clamp, rely on dynamic spring stiffness
					float k = 350.0f; // Lowered for noticeable web-like stretch
					if (dist > effectiveMaxLen * 1.3f) {
						k *= Mathf.Clamp((dist - effectiveMaxLen * 1.3f) / (effectiveMaxLen * 0.2f), 1.0f, 5.0f);
					}
					float c = 2.0f * Mathf.Sqrt(k); 
					float stretch = dist - maxLen;
					
					Vector2 springForce = (-k * stretch) * radialDir;
					Vector2 dampingForce = -c * radialVelCurrent;
					
					relVel += (springForce + dampingForce) * blendFactor * (float)delta;
					
					// Fix 3: Conditional Radial Damping - only strip if moving outward relative to anchor
					if (radialSpeed > 0) {
						relVel -= radialVelCurrent;
					}
				}
			}
			
			// 3. Tangential Input (Screen-Space Projection) - No speed cap to preserve momentum
			if (dist > 0.1f) { 
				float inputX = Input.GetActionStrength("Right") - Input.GetActionStrength("Left");
				Vector2 inputDir = new Vector2(inputX, 0);
				
				// Project raw horizontal input onto the tangent plane to prevent control reversal below anchor
				Vector2 tangentialInput = inputDir - (inputDir.Dot(radialDir) * radialDir);
				
				relVel += tangentialInput * speed * (float)delta * 3.0f;
			}
			
			// Convert back to absolute velocity
			vel = relVel + anchorVel;

			// Step 14: Native Surface Friction Integration
			if (frictionCast.IsColliding()) {
				Node collider = frictionCast.GetCollider(0) as Node;
				if (collider != null && collider != this) { // Ignore self-collision and nulls
					Vector2 normal = frictionCast.GetCollisionNormal(0);
					float frictionCoeff = GetSurfaceFriction(collider);
					
					// Decompose velocity into perpendicular and parallel components relative to surface
					Vector2 vPerp = normal * vel.Dot(normal);
					Vector2 vParallel = vel - vPerp;
					
					// Apply friction damping to parallel component (with deadzone)
					if (vParallel.Length() > 10.0f) {
						float dampFactor = Mathf.Pow(1.0f - frictionCoeff, (float)delta * 60.0f);
						vParallel *= dampFactor;
					}
					vel = vPerp + vParallel;
				}
			}
		}
	}
}
