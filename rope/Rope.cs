using Godot;
using System;

public partial class Rope : Node {
	public const float Speed = 50f; // The speed with which the chain moves
	private Vector2 direction = new Vector2(0, 0);
	private Vector2 tip = new Vector2(0, 0);

	private bool flying = false;
	private bool hooked = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		//var tipLoc = Vector2.ToLocal();
	}
	
	public void shoot(Vector2 dir) {
		direction = dir.Normalized();
		flying = true;
		//tip = GlobalPosition;
	}

	public void release() {
		flying = false;
		hooked = false;
	}
}
