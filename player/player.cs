using Godot;
using System;

public partial class player : CharacterBody2D
{
	private const float Speed = 300.0f;
	private const float JumpVelocity = -400.0f;
	private Vector2 hookDir = Vector2.Zero;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	private float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	private RayCast2D rayCast; 
	private const float RaycastLength = 105.0f;
	private bool isGrappled = false;

	public PackedScene HookScene; // Reference to the Hook scene
	
	public override void _Ready() {
		rayCast = GetNode<RayCast2D>("RayCast2D");
		rayCast.Enabled = true;  // disabled by default, we'll turn it on when we clck
	}

	public override void _PhysicsProcess(double delta) {
		/*
		Vector2 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y += gravity * (float)delta;

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
			velocity.Y = JumpVelocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		if (direction != Vector2.Zero)
		{
			velocity.X = direction.X * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
		*/
	}

	public override void _Input(InputEvent @event) {
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left) {
			// i will probably need this raycast later to check if there's an object in between the starting pos of the gun and the player
			// Get the global position of the mouse click
			
			Vector2 mousePosition = GetGlobalMousePosition();
			Vector2 direction = (mousePosition - GlobalPosition).Normalized();
			direction *= RaycastLength;  // need to rename this later!!!
			
			// Set the raycast's target position relative to the character's position
			rayCast.TargetPosition = direction;  
			
			// Optionally update the raycast (not needed if auto_update is true)
			rayCast.ForceRaycastUpdate();

			if (rayCast.IsColliding()) {
				GD.Print("collided! distance: " + GlobalPosition.DistanceTo(rayCast.GetCollisionPoint()));
			} else {
				GD.Print("did not collide with anything.");
			}
			
			if (HookScene == null) {
				GD.PrintErr("HookScene is not set in the Player node.");
				return;
			}

		}
	}

	public Vector2 GetRaycastPos() {
		return rayCast.GlobalPosition;
	}
}
