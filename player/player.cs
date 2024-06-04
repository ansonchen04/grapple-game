using Godot;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;

public partial class player : CharacterBody2D
{
	public const float Speed = 300.0f;
	public const float JumpVelocity = -600.0f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	private RayCast2D _downwardRaycast;
	bool onClimbableSurface = false;
	bool onOneWaySurface = false;
    public override void _Ready()
    {
        // Initialize the RayCast2D node
        _downwardRaycast = GetNode<RayCast2D>("DownwardRaycast");
    }
	public override void _PhysicsProcess(double delta)
	{
        // Check if the player is on the floor or a specific platform using the raycast
        if (_downwardRaycast.IsColliding())
        {
			GD.Print(_downwardRaycast.GetCollider());
            var collider = _downwardRaycast.GetCollider();
            if (collider is Node2D platform)
            {	
				//TODO Fix this portion of the code
                GD.Print($"Standing on platform: {platform.Name}");
				Node[] children = platform.FindChildren("*","CollisionPolygon2D",false,false).ToArray();
				if(children.Length == 0){
					children = platform.FindChildren("*","CollisionShape2D",true,false).ToArray();
					CollisionShape2D collisionPolygon = (CollisionShape2D)children[0];
				if (collisionPolygon != null)
                {
					if(collisionPolygon.OneWayCollision == true){
						onOneWaySurface = true;
					}
					else{
						onOneWaySurface = false;
					}
				}
				}
				else{
					CollisionPolygon2D collisionPolygon = (CollisionPolygon2D)children[0];
					if (collisionPolygon != null)
                {
					if(collisionPolygon.OneWayCollision == true){
						onOneWaySurface = true;
					}
					else{
						onOneWaySurface = false;
					}
				}
				}
				
            }

		}
		//Gets the current velocity
		Vector2 newVelocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			newVelocity.Y += gravity * (float)delta;
		//Checking which movement option, if any, is being used
		if(onOneWaySurface){
				onewaydropMovement(newVelocity);
		}
		else if(onClimbableSurface){

		}
		//If nothing fancy, just use base movement vectors
		newVelocity = baseMovement(newVelocity);
		//Updates to the new velocity
		Velocity = newVelocity;
		//Moves the sprite at the end
		MoveAndSlide();
	}
	private Vector2 baseMovement(Vector2 newVelocity)
	{
		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 direction = Input.GetVector("Left", "Right", "Up", "Down");
		if (direction != Vector2.Zero)
		{
			newVelocity.X = direction.X * Speed;
		}
		else
		{
			newVelocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		}
		// Handle Jump.
		if (Input.IsActionJustPressed("Up") && IsOnFloor())
			newVelocity.Y = JumpVelocity;
		return newVelocity;
	}
	/*private Vector2 climbMovement(Vector2 velocity){

	}*/
	private Vector2 onewaydropMovement(Vector2 velocity){
		// Drop the player down 1 pixel if standing on a one-way collision platform and "ui_drop_down" is pressed
        //TODO verify this actually does ^
		if (IsOnFloor() && Input.IsActionJustPressed("Down"))
        {
            Position += new Vector2(0, 1);
        }
		return velocity;
	}
	public void setClimbing(bool onClimbableSurface){
        this.onClimbableSurface = onClimbableSurface;
	}
	public void setOneWay(bool onOneWaySurface){
		this.onOneWaySurface = onOneWaySurface;
	}
}