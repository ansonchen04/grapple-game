using Godot;
using System;

// for some reason this class name needs to be lowercase. it does not work in uppercase.
public partial class hook : RigidBody2D
{
	public const float Speed = 100f; // The speed with which the hook moves
	private Vector2 direction = new Vector2(0, 0);
	private float dist = 0f;
	private Area2D collisionArea;
	private CollisionShape2D collisionShape;
	private CollisionShape2D collision;
	private Marker2D anchor;
	private Marker2D handle;
	private bool connected = false;
	
	[Signal] public delegate void HookCollisionEventHandler();

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
		
		anchor = GetNode<Marker2D>("RopeAnchor");
		handle = GetNode<Marker2D>("RopeHandle");
		Node2D rope = GetNode<Node2D>("../Player/Rope");
		//GD.PushError("(tried) to find rope node!");
		anchor.Call("set_rope_path", rope.GetPath());
		handle.Call("set_rope_path", rope.GetPath());
		hookState = HookState.Hidden;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta) {
		// depending on what state we're in, the hook acts (and moves) differently
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
				LinearVelocity = Vector2.Zero;
				break;
			case HookState.Retracting:
				break;
		}
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

	// this function is called when the hook hits something
	private void OnBodyEntered(Node2D body) {
		if (hookState == HookState.Shot) {
			// GD.Print("Collided with: " + body.Name);
			if (body.IsInGroup("NotHookable")) {
				// GD.Print("not hookable!");
				return; 
			}

			// the hook should freeze when we hit something. also shouldn't be able to be moved around.
			Mass = 999999999;  // freeze is not working so this is the temporary solution
			GD.Print("frozen!");
			hookState = HookState.Hooked;
			EmitSignal(SignalName.HookCollision);
		}
	}

	// show the hook (when shot)
	private void showHook() {
		Show();
		SetPhysicsProcess(true);
		collisionShape.Disabled = false;
		collision.Disabled = false;
	}
}
