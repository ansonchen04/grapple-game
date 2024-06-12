using Godot;
using System;

// for some reason this class name needs to be lowercase. it does not work in uppercase.
public partial class hook : RigidBody2D
{
	public const float Speed = 100f; // The speed with which the chain moves
	private Vector2 direction = new Vector2(0, 0);
	private float dist = 0f;
	private float deltaDist = 0f;

	private bool flying = false;
	private bool hooked = false;
	private Area2D collisionArea;
	private CollisionShape2D collisionShape;
	private PinJoint2D pinJoint;

	private RigidBody2D lastRopePiece;
	private Vector2 lastPos;
	int id;  // for the rope

	int tempMax = 5;
	private CharacterBody2D player;
	const float PieceLen = 16;

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

	}

	// Shoot the grapple
	public void Shoot(Vector2 targetPosition) {
		flying = true;
		direction = (targetPosition - GlobalPosition).Normalized();
		GlobalPosition += direction * 105;
	}

	// when the hook hits something
	private void OnBodyEntered(Node2D body) {
		// GD.Print("Collided with: " + body.Name);
		if (body.IsInGroup("NotHookable")) {
			// GD.Print("not hookable!");
			return; // Ignore this collision
		}
		flying = false;
		hooked = true;

		//Freeze = true;
		//FreezeMode = FreezeModeEnum.Static;
		Mass = 999999999;  // freeze is not working so this is the temporary solution
		//GD.Print("frozen!");

		// Additional logic when the hook hits something can go here
	}

	// adds a new piece attached to this piece
	private RigidBody2D AddNewPiece(float angle) {
		PackedScene ropePieceScene = (PackedScene)ResourceLoader.Load("res://rope/rope_piece.tscn");
		RigidBody2D ropePiece = (RigidBody2D)ropePieceScene.Instantiate();

		//GD.Print("summoned a new piece (hook)");
		
		// Add the rope piece to the scene
		GetParent().AddChild(ropePiece);
		Vector2 vector22 = CreateVector(18 + PieceLen / 2, angle);
		ropePiece.GlobalPosition += vector22;  // 18 is the size of the hitbox + 4 for half the len of the ropepiece
		ropePiece.Rotation = angle - (float) Math.PI / 2;

		ropePiece.Call("SetParent", this);

		pinJoint.NodeA = GetPath();
		pinJoint.NodeB = ropePiece.GetPath();
		return ropePiece;
	}

	// helper function that creates a vector of length length and in direction angleInRadians
	private Vector2 CreateVector(float length, float angleInRadians) {
        float x = length * Mathf.Cos(angleInRadians);
        float y = length * Mathf.Sin(angleInRadians);

        return new Vector2(x, y);
    }

	public void ClearJoint() {
		pinJoint.NodeA = GetPath();
		pinJoint.NodeB = null;
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

	public Vector2 GetJointPos() {
		return pinJoint.GlobalPosition;
	}
}
