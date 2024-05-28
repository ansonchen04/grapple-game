using Godot;
using System;

// for some reason this class name needs to be lowercase. it does not work in uppercase.
public partial class hook : RigidBody2D
{
	public const float Speed = 100f; // The speed with which the chain moves
	private Vector2 direction = new Vector2(0, 0);
	private float dist = 0f;

	private Area2D collisionArea;
	private CollisionShape2D collisionShape;
	private CollisionShape2D collision;

	public enum HookState {
		Hidden,
		Shot,
		Hooked,
		Retracting
	}
	private HookState hookState;	


	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		collisionArea = GetNode<Area2D>("Area2D");
		collisionShape = GetNode<CollisionShape2D>("Area2D/CollisionShape2D");
		collision = GetNode<CollisionShape2D>("Collision");
		collisionArea.Connect("body_entered", new Callable(this, nameof(OnBodyEntered)));
		hookState = HookState.Hidden;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta) {
		// Move the hook in the set direction
		switch (hookState) {
			case HookState.Hidden:
				Hide();
				SetPhysicsProcess(false);
				collisionShape.Disabled = true;
				collision.Disabled = true;
				break;
			case HookState.Shot:
				LinearVelocity = direction * Speed;
				break;
			case HookState.Hooked:
				break;
			case HookState.Retracting:
				break;
		}
		//LinearVelocity = direction * Speed;
	}

	// Shoot the grapple
	public void Shoot(Vector2 targetPosition, Vector2 playerPosition) {
		if (hookState == HookState.Hidden) {
			GlobalPosition = playerPosition;
			direction = (targetPosition - GlobalPosition).Normalized();
			GlobalPosition += direction * 105;
			hookState = HookState.Shot;
			showHook();
		}
	}

	private void OnBodyEntered(Node2D body) {
		if (hookState == HookState.Shot) {
			// GD.Print("Collided with: " + body.Name);
			if (body.IsInGroup("NotHookable")) {
				// GD.Print("not hookable!");
				return; // Ignore this collision
			}

			//Freeze = true;
			//FreezeMode = FreezeModeEnum.Static;
			Mass = 999999999;  // freeze is not working so this is the temporary solution
			GD.Print("frozen!");
			hookState = HookState.Hooked;
			
			// Additional logic when the hook hits something can go here
		}
	}

	private void showHook() {
		Show();
		//SetPhysicsProcess(true);
		collisionShape.Disabled = false;
		collision.Disabled = false;
	}
}
