using Godot;
using System;

public partial class RopePiece : RigidBody2D {
	// Called when the node enters the scene tree for the first time.
	//RigidBody2D nextPiece;
	PinJoint2D pinJoint;
	//PackedScene ropePieceScene;
	static int len;
	int id;
	const float PieceLen = 16;


	public override void _Ready() {
		pinJoint = GetNode<PinJoint2D>("PinJoint2D");
		//ropePieceScene = (PackedScene) ResourceLoader.Load("res://rope/rope_piece.tscn");
		++len;
		id = len;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		
	}

	public RigidBody2D AddNewPiece(float angle) {
		// make a new RopePiece here, and then i want to connect pinJoint.NodeB to it
		GD.Print("summoned a new piece");
		PackedScene ropePieceScene = (PackedScene) ResourceLoader.Load("res://rope/rope_piece.tscn");
		RigidBody2D newPiece = (RigidBody2D) ropePieceScene.Instantiate();
		GetParent().AddChild(newPiece);
		Vector2 vector8 = CreateVector(PieceLen, angle);
		newPiece.GlobalPosition = GlobalPosition + vector8;
		//newPiece.GlobalPosition = pinJoint.GlobalPosition;  // might have to fix the positioning

		// set position? do it here if we need to
        newPiece.Rotation = angle - (float) Math.PI / 2;

		pinJoint.NodeA = GetPath();
		pinJoint.NodeB = newPiece.GetPath();
		return newPiece;
	}

	public void ConnectToPlayer(CharacterBody2D player) {
		pinJoint.NodeA = GetPath();
		pinJoint.NodeB = player.GetPath();
	}

	private Vector2 CreateVector(float length, float angleInRadians) {
        float x = length * Mathf.Cos(angleInRadians);
        float y = length * Mathf.Sin(angleInRadians);

        return new Vector2(x, y);
    }

	public void SetId(int newId) {
		id = newId;
	}

	public int GetId() {
		return id;
	}
}
