using Godot;
using System;

public partial class hook : RigidBody2D
{
	public const float Speed = 100f; // The speed with which the chain moves
	private Vector2 direction = new Vector2(0, 0);
	private bool flying = false;
	private bool hooked = false;
	private Area2D collisionArea;
	private CollisionShape2D collisionShape;
	private PinJoint2D pinJoint;

	private RigidBody2D lastRopePiece;
	private Vector2 lastPos;
	int id;  // for the rope

	private CharacterBody2D player;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		collisionArea = GetNode<Area2D>("Area2D");
		collisionShape = GetNode<CollisionShape2D>("Area2D/CollisionShape2D");
		pinJoint = GetNode<PinJoint2D>("PinJoint2D");
		collisionArea.Connect("body_entered", new Callable(this, nameof(OnBodyEntered)));
		lastRopePiece = this;
		lastPos = GlobalPosition;
		//player = GetParent().GetNode<CharacterBody2D>("player"); // Adjust the path to your player node
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta) {
		// Move the hook in the set direction
		//if (flying) {
		//	Position += direction * Speed * (float)delta;
		//}
	}

	// Shoot the grapple
	public void Shoot(Vector2 shootDirection) {
		Mass = 9999999999;  // prevents other forces from acting on the hook
		flying = true;
		direction = shootDirection;
		LinearVelocity = shootDirection * Speed;
	}

	private void OnBodyEntered(Node2D body) {
		if (body.IsInGroup("NotHookable")) {
			return; // Ignore this collision
		}

		GD.Print("on body is entered");

		flying = false;
		hooked = true;

		//Mass = 999999999;  // freeze is not working so this is the temporary solution
		FreezeMode = RigidBody2D.FreezeModeEnum.Static;

		// Additional logic when the hook hits something can go here
	}

	private RigidBody2D AddNewPiece(Vector2 offset) {
		PackedScene ropePieceScene = (PackedScene)ResourceLoader.Load("res://rope/rope_piece.tscn");
		RigidBody2D ropePiece = (RigidBody2D)ropePieceScene.Instantiate();

		GD.Print("summoned a new piece (hook)");
		
		// Add the rope piece to the scene
		GetParent().AddChild(ropePiece);
		ropePiece.GlobalPosition += offset;

		pinJoint.NodeA = GetPath();
		pinJoint.NodeB = ropePiece.GetPath();
		return ropePiece;
	}

	public void ConnectToPlayer(CharacterBody2D player) {
		GD.Print("connected to player!");
		pinJoint.NodeA = GetPath();
		pinJoint.NodeB = player.GetPath();
	}

	public bool IsFlying() {
		return flying;
	}

	public void SetId(int newId) {
		id = newId;
	}

	public int GetId() {
		return id;
	}

	public void HideHook() {
		// Hide the hook from view
		Visible = false;

		// Disable collision interactions
		collisionArea.SetDeferred("monitoring", false);  // Disable monitoring collisions
		collisionShape.Disabled = true;  // Disable collision shape

		// Disable physics temporarily
		FreezeMode = RigidBody2D.FreezeModeEnum.Static;
	}

	public void ShowHook() {
		Visible = true;

		collisionArea.SetDeferred("monitoring", true);  // Disable monitoring collisions
		collisionShape.Disabled = false;  // Disable collision shape
		
		FreezeMode = RigidBody2D.FreezeModeEnum.Kinematic;
	}
}
