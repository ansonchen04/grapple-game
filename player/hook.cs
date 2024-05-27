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
	private DampedSpringJoint2D springJoint;

	private RigidBody2D lastRopePiece;
	private Vector2 lastPos;

	int tempMax = 5;
	private CharacterBody2D player;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		collisionArea = GetNode<Area2D>("Area2D");
		collisionShape = GetNode<CollisionShape2D>("Area2D/CollisionShape2D");
		collisionArea.Connect("body_entered", new Callable(this, nameof(OnBodyEntered)));
		springJoint = GetNode<DampedSpringJoint2D>("DampedSpringJoint2D");
		lastRopePiece = this;
		lastPos = GlobalPosition;
		player = GetParent().GetNode<CharacterBody2D>("player"); // Adjust the path to your player node
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta) {
		// Move the hook in the set direction
		if (flying) {
			GlobalPosition += direction * Speed * (float)delta;
			dist += Speed * (float)delta;
			deltaDist += Speed * (float)delta;
			if (tempMax > 0) {
				if (deltaDist >= 100) {  // sample distance
					AddRopePiece();    
					deltaDist = 0;
					tempMax--;
				}
				lastPos = lastRopePiece.GlobalPosition - direction * 25;  // arbitrary number for now
				GD.Print("distance: " + dist);
			} else {
				AttachPlayer();
			}
		} else if (hooked) {
			AttachPlayer();
		}
	}

	// Shoot the grapple
	public void Shoot(Vector2 targetPosition) {
		flying = true;
		direction = (targetPosition - GlobalPosition).Normalized();
		GlobalPosition += direction * 105;
	}

	private void OnBodyEntered(Node2D body) {
		// GD.Print("Collided with: " + body.Name);
		if (body.IsInGroup("NotHookable")) {
			// GD.Print("not hookable!");
			return; // Ignore this collision
		}
		flying = false;
		hooked = true;

		Freeze = true;
		FreezeMode = FreezeModeEnum.Static;
		Mass = 999999999;  // freeze is not working so this is the temporary solution
		GD.Print("frozen!");

		// Additional logic when the hook hits something can go here
	}

	private void AddRopePiece() {
		PackedScene ropePieceScene = (PackedScene)ResourceLoader.Load("res://rope/rope_piece.tscn");
		RigidBody2D ropePiece = (RigidBody2D)ropePieceScene.Instantiate();
		
		// Add the rope piece to the scene
		GetParent().AddChild(ropePiece);
		ropePiece.GlobalPosition = lastPos;
		
		// Set the velocity of the new rope piece to match the hook's velocity
		//ropePiece.LinearVelocity = direction * Speed;

		// Attach the new rope piece to the previous piece or the hook
		if (lastRopePiece == this) {
			AttachNewPiece(ropePiece);
		} else {
			lastRopePiece.Call("AttachNewPiece", ropePiece);
		}
		
		lastRopePiece = ropePiece;
	}

	public void AttachNewPiece(RigidBody2D ropePiece) {
		// Create a new DampedSpringJoint2D for the new rope piece
		DampedSpringJoint2D newSpringJoint = new DampedSpringJoint2D();
		newSpringJoint.Position = Position;  // Adjust this if needed
		newSpringJoint.NodeA = lastRopePiece.GetPath();
		newSpringJoint.NodeB = ropePiece.GetPath();
		
		// Add the new spring joint to the parent node
		GetParent().AddChild(newSpringJoint);
		
		// Adjust the spring joint properties if needed
		newSpringJoint.Stiffness = 64; //springJoint.Stiffness;
		newSpringJoint.Damping = 16; //springJoint.Damping;
	}

	private void AttachPlayer() {
		// Create a new DampedSpringJoint2D to attach the player to the rope piece
		DampedSpringJoint2D playerSpringJoint = new DampedSpringJoint2D();
		playerSpringJoint.Position = player.GlobalPosition;  // Adjust this if needed
		playerSpringJoint.NodeA = player.GetPath();
		playerSpringJoint.NodeB = lastRopePiece.GetPath();

		// Add the player spring joint to the parent node
		GetParent().AddChild(playerSpringJoint);

		// Adjust the spring joint properties if needed
		playerSpringJoint.Stiffness = springJoint.Stiffness;
		playerSpringJoint.Damping = springJoint.Damping;
	}

	public bool IsFlying() {
		return flying;
	}
}
