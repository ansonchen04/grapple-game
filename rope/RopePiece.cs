using Godot;
using System;

public partial class RopePiece : RigidBody2D {
	// Called when the node enters the scene tree for the first time.
	//RigidBody2D nextPiece;
	PinJoint2D pinJoint;
	//DampedSpringJoint2D pinJoint;
	//PackedScene ropePieceScene;
	static int len;
	int id;


	public override void _Ready() {
		pinJoint = GetNode<PinJoint2D>("PinJoint2D");
		//pinJoint = GetNode<DampedSpringJoint2D>("DampedSpringJoint2D");
		//ropePieceScene = (PackedScene) ResourceLoader.Load("res://rope/rope_piece.tscn");
		++len;
		id = len;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		
	}

	public RigidBody2D AddNewPiece(Vector2 offset) {
		// make a new RopePiece here, and then i want to connect pinJoint.NodeB to it
		//GD.Print("summoned a new piece");
		PackedScene ropePieceScene = (PackedScene) ResourceLoader.Load("res://rope/rope_piece.tscn");
		RigidBody2D newPiece = (RigidBody2D) ropePieceScene.Instantiate();
		GetParent().AddChild(newPiece);
		newPiece.GlobalPosition = GlobalPosition + offset;

		// set position? do it here if we need to

		pinJoint.NodeA = GetPath();
		pinJoint.NodeB = newPiece.GetPath();
		return newPiece;
	}

	public void ConnectToPlayer(CharacterBody2D player) {  // fix...?
		//GD.Print("connected to player!");
		pinJoint.NodeA = GetPath();
		pinJoint.NodeB = player.GetPath();
	}

	public void SetId(int newId) {
		id = newId;
	}

	public int GetId() {
		return id;
	}
}
