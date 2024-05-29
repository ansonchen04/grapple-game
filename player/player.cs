using Godot;
using System;

public partial class Player : CharacterBody2D
{
	private const float Speed = 300.0f;
	private const float JumpVelocity = -400.0f;
	private Vector2 hookDir = Vector2.Zero;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	private float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	private RayCast2D rayCast; 
	private const float RaycastLength = 105.0f;
	private bool isGrappled = false;

	private RigidBody2D hook;
	private Node2D rope;

	bool bruh = false;
	
	public override void _Ready() {
		rayCast = GetNode<RayCast2D>("RayCast2D");
		rayCast.Enabled = true; 
		hook = GetNode<RigidBody2D>("../Hook");
		hook.Connect("HookCollision", new Callable(this, nameof(OnHookCollision)));
		rope = GetNode<Node2D>("Rope");
		hideRope();
	}

	public override void _PhysicsProcess(double delta) {
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
		if (direction != Vector2.Zero) {
			velocity.X = direction.X * Speed;
		} else {
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();

		if (bruh) {
			float len = (float) rope.Call("get_length");
			rope.Call("_set_length", len + 2f);
		}
	}

	
	public override void _Input(InputEvent @event) {
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left) {
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


			// shoot and show the rope + hook
			hook.Call("Shoot", mousePosition, GlobalPosition);
			showRope();
			bruh = true;
			// technically i'm not supposed to call this as it's a private func but idc
			//float len = 100.0f;

			//rope.Call("_set_length", 800.0f);
		}
	}

	private void hideRope() {
		rope.Hide();
		rope.SetPhysicsProcess(false);
		rope.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
	}

	private void showRope() {
		rope.Show();
		rope.SetPhysicsProcess(true);
		rope.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
	}

	private int ropeRate(float hookSpeed) {
		return 0;
	}

	private void OnHookCollision() {
        GD.Print("Hook collided with something!");
        bruh = false;
        // Additional code based on game requirements
    }
	
}
