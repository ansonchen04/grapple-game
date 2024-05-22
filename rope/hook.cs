using Godot;
using System;

public partial class hook : RigidBody2D
{
	public const float Speed = 999f; // The speed with which the chain moves
	private Vector2 direction = new Vector2(0, 0);
	private Vector2 tip = new Vector2(0, 0);

	private bool flying = false;
	private bool hooked = false;
	private RayCast2D rayCast;
	// Player player;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		rayCast = GetNode<RayCast2D>("RayCast2D");  // pretty much j for debug
		//player = GetNode<Player>("player/Player");
		rayCast.Enabled = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta) {
		// Move the hook in the set direction
		if (flying) {
			GlobalPosition += direction * Speed * (float) delta;
		}
	}

	public void Shoot(Vector2 TargetPosition) {
		flying = true;
		
		//Vector2 playerPos = player.GlobalPosition;
		direction = (TargetPosition - GlobalPosition).Normalized();
		GlobalPosition += direction * 105;
		// Set the raycast's target position relative to the character's position
		rayCast.TargetPosition = direction;
		
		// Optionally update the raycast (not needed if auto_update is true)
		rayCast.ForceRaycastUpdate();
		
	}
}
