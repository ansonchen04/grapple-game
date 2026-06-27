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
	}
	public override void _PhysicsProcess(double delta) {
		GD.Print(Position.Y);
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
        } else {
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
		if (direction != Vector2.Zero) {
			velocity.X = direction.X * speed;
		}
		else {
			velocity.X = Mathf.MoveToward(Velocity.X, 0, speed);
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

    void ApplySwingPhysics(ref Vector2 vel, double delta) {
        Vector2 anchor = rope.GetAnchor();
        float maxLen = rope.GetMaxRopeLength();
        Vector2 toPlayer = GlobalPosition - anchor;
        float dist = toPlayer.Length();
        
        // 1. Gravity
        vel.Y += gravity * (float)delta;
        
        // 2. Spring/Damping Constraint
        if (dist > maxLen && dist > 0.001f) {
            Vector2 radialDir = toPlayer / dist;
            Vector2 radialVel = vel.Dot(radialDir) * radialDir;
            
            // Blend window: 0 to full strength over 10% of maxLen to prevent velocity "pops"
            float blendStart = maxLen;
            float blendEnd = maxLen * 1.1f;
            float blendFactor = Mathf.Clamp((dist - blendStart) / (blendEnd - blendStart), 0.0f, 1.0f);
            
            // Hard clamp at 1.3x to prevent infinite stretch/overshoot
            if (dist > maxLen * 1.3f) {
                GlobalPosition = anchor + radialDir * (maxLen * 1.3f);
                toPlayer = GlobalPosition - anchor;
                dist = maxLen * 1.3f;
            }
            
            // Critically damped spring force (mass assumed ~1 for simplicity)
            float k = 800.0f; 
            float c = 2.0f * Mathf.Sqrt(k); 
            
            float stretch = dist - maxLen;
            Vector2 springForce = (-k * stretch) * radialDir;
            Vector2 dampingForce = -c * radialVel;
            
            vel += (springForce + dampingForce) * blendFactor * (float)delta;
            
            // Remove remaining radial velocity to prevent bouncing
            vel -= radialVel;
        }
        
        // 3. Tangential Input with speed cap & falloff
        if (dist > 0.001f) {
            Vector2 radialDir = toPlayer / dist;
            Vector2 tangentDir = new Vector2(-radialDir.Y, radialDir.X);
            float inputX = Input.GetActionStrength("Right") - Input.GetActionStrength("Left");
            
            float currentTangentialSpeed = Mathf.Abs(vel.Dot(tangentDir));
            float maxTangentialSpeed = 600.0f;
            // Velocity-dependent acceleration falloff prevents infinite energy pumping
            float accelFalloff = Mathf.Clamp((maxTangentialSpeed - currentTangentialSpeed) / (maxTangentialSpeed * 0.5f), 0.0f, 1.0f);
            
            vel += tangentDir * inputX * speed * accelFalloff * (float)delta * 3.0f;
        }
    }
}
