using Godot;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;

public partial class player : CharacterBody2D
{

	//How fast the player moves and how high they can jump
	private const float Speed = 300.0f;
	private const float JumpVelocity = -600.0f;
	private const float ClimbVelocity = -200.0f;
	//Starting Position, should be updated whenever player enters a new scene
	private Vector2 startPosition;
	private Vector2 OOB = new Vector2(4500,2500);
  private Vector2 hookStartPos;
	// Get the gravity from the project settings to be synced with RigidBody nodes.
	private float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	//Ray is in the center of the player model, checking what platform the player is on
	private RayCast2D _downwardRaycast;
  //TODO Rename raycast length to a clearer name
  private const float RaycastLength = 105.0f;
	private bool isGrappled = false;
  Node2D rope;
	Vector2 ropePull;
	
	//Booleans to check if we are on a special surface, if we have different movement options
	bool onClimbableSurface = false;
	bool onOneWaySurface = false;
    public override void _Ready()
    {
        // Initialize the RayCast2D node
        _downwardRaycast = GetNode<RayCast2D>("DownwardRaycast");
		    startPosition = this.GlobalPosition;
		    GD.Print(startPosition);
        rope = GetNode<Node2D>("../Rope");  // you need a rope in each scene with a player
		    ropePull = Vector2.Zero;
    }
	public override void _PhysicsProcess(double delta)
	{
		//This is shit code and will be replaced once I figure out resources
		if(Position.X > OOB.X || Position.Y > OOB.Y || Input.IsActionJustPressed("Restart")){
		this.restart();
		}
        // Check if the player is on the floor or a specific platform using the raycast
        if (_downwardRaycast.IsColliding())
        {
            var collider = _downwardRaycast.GetCollider();
			//Likely will have to constrain this more
            if (collider is Node2D platform)
            {	
				//Will likely have to safeguard this if we collide with platforms without a collision box. 
				//Not sure when that would happen though
				this.setOneWay(this.checkOneway(platform));
            }
		}
		//Gets the current velocity
		Vector2 newVelocity = Velocity;
		//Checking which movement option, if any, is being used. Will convert this into a switch case in a future commit
		if(onOneWaySurface){
			onewaydropMovement(newVelocity);
			newVelocity = baseMovement(newVelocity);
		}
		else if(onClimbableSurface){
			newVelocity = climbMovement(newVelocity);
		}
		else{
		//If nothing fancy, just use base movement vectors
		newVelocity = baseMovement(newVelocity);
		}
		// Add the gravity.
		if (!IsOnFloor() && !onClimbableSurface)
			newVelocity.Y += gravity * (float)delta;
    ropePull = (Vector2) rope.Call("GetPull");
		newVelocity += ropePull;
		//Updates to the new velocity
		Velocity = newVelocity;
		//Moves the sprite at the end
		MoveAndSlide();
	}
  	public override void _Input(InputEvent @event) {
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left) {
			// i will probably need this raycast later to check if there's an object in between the starting pos of the gun and the player
			// Get the global position of the mouse click
			
			Vector2 mousePosition = GetGlobalMousePosition();
			Vector2 direction = (mousePosition - GlobalPosition).Normalized();
			direction *= RaycastLength;  // need to rename this later!!!

			hookStartPos = direction;
			rope.Call("SetMouseLoc", direction);
			
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
	private Vector2 baseMovement(Vector2 velocity)
	{
		// Get the input direction and handle the movement/deceleration.
		Vector2 direction = Input.GetVector("Left", "Right", "Up", "Down");
		if (direction != Vector2.Zero)
		{
			velocity.X = direction.X * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		}
		// Handle Jump.
		if (Input.IsActionJustPressed("Up") && IsOnFloor())
			velocity.Y = JumpVelocity;
		return velocity;
	}
	private Vector2 climbMovement(Vector2 velocity){
		// Get the input direction and handle the movement/deceleration.
		Vector2 direction = Input.GetVector("Left", "Right", "Up", "Down");
		if (direction != Vector2.Zero)
		{
			velocity.X = direction.X * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		}
	private Vector2 onewaydropMovement(Vector2 velocity){
		// Drop the player down 1 pixel if standing on a one-way collision platform and "ui_drop_down" is pressed
        //TODO verify this actually does ^
		if (IsOnFloor() && Input.IsActionJustPressed("Down"))
        {
            Position += new Vector2(0, 1);
        }
		return velocity;
	}
	private Boolean checkOneway(Node2D platform){
		//Gets the collision polygon or collision shape of the platform
		Node[] children = platform.FindChildren("*","CollisionPolygon2D",false,false).ToArray();
		if(children.Length == 0){
			children = platform.FindChildren("*","CollisionShape2D",true,false).ToArray();
			CollisionShape2D collisionPolygon = (CollisionShape2D)children[0];
			if (collisionPolygon != null && collisionPolygon.OneWayCollision == true)
            {
				return true;
			}
			}
		else{
			CollisionPolygon2D collisionPolygon = (CollisionPolygon2D)children[0];
				if (collisionPolygon != null)
				{
					if(collisionPolygon.OneWayCollision == true){
						return true;
					}
				}
				}
		return false;
	}
	public void restart(){
		//If restart button pressed, reset the positon to the start and zero out the velocity
			Position = startPosition;
			Velocity = Vector2.Zero;
	}
	public void setClimbing(bool onClimbableSurface){
        this.onClimbableSurface = onClimbableSurface;
	}
	public void setOneWay(bool onOneWaySurface){
		this.onOneWaySurface = onOneWaySurface;
	}
  // sets the force the rope should be applying to the player
	public void SetRopePull(Vector2 newRopePull) {
		ropePull = newRopePull;
	}

	public Vector2 GetRaycastPos() {
		return rayCast.GlobalPosition;
	}

	public Vector2 GetHookStartPos() {
		return hookStartPos + GlobalPosition;
		// Handle Jump.
		if (Input.IsActionPressed("Up"))
			velocity.Y = ClimbVelocity;
		else if (Input.IsActionPressed("Down"))
			velocity.Y = -ClimbVelocity;
		else
			velocity.Y = 0;
		return velocity;
	}
}