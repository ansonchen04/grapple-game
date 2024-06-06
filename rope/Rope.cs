using Godot;
using System;

public partial class Rope : Node2D {
    RigidBody2D hook;
    RigidBody2D firstPiece;
    RigidBody2D lastPiece;
	CharacterBody2D player;
    bool connectedToPlayer = false;
    RopeState ropeState;
    int len = 0;
    const int MaxLength = 100;  // max num links in the rope
	RigidBody2D[] ropePieces;
    float moveSpeed = 300f;  // Speed of the rope movement

    public override void _Ready() {
        hook = GetNode<RigidBody2D>("Hook");
		player = GetNode<CharacterBody2D>("../Player");  // assuming the player is always in the scene
        firstPiece = hook;
        lastPiece = hook;
        ropeState = RopeState.Hidden;
		ropePieces = new RigidBody2D[MaxLength];
    }

    public override void _Process(double delta) {
        switch (ropeState) {
            case RopeState.Hidden:
                if (len > 1) {
                    ClearRope();
                }
                hook.Call("HideHook");
                break;
            case RopeState.Shot:
                float dist = player.GlobalPosition.DistanceTo(hook.GlobalPosition);
                while (len < dist / 30 && len < MaxLength) {  // add the rope collision thing later
                    lastPiece.Call("SetId", len);
                    lastPiece = (RigidBody2D) lastPiece.Call("AddNewPiece", new Vector2(0, 10));
                    if (len == 1) {
                        //lastPiece.GlobalPosition = hook.GlobalPosition;
                        firstPiece = lastPiece;
                    }
					ropePieces[len] = lastPiece;
                    len++;
                    connectedToPlayer = false;
                } 

                if (!connectedToPlayer) {
                    lastPiece.Call("ConnectToPlayer", player);
                    lastPiece.GlobalPosition = player.GlobalPosition;
                    connectedToPlayer = true;
                }

                firstPiece.GlobalPosition = hook.GlobalPosition;
                lastPiece.GlobalPosition = player.GlobalPosition;
				
				if (Input.IsActionJustPressed("ui_accept")) {  
                    ClearRope();
                }
                break;
            case RopeState.Hooked:
                // stop growing the rope. lock the hook in place.
                break;
            case RopeState.Retracting:
                break;
            case RopeState.Slack:
                break;
        }
    }
    
    public override void _Input(InputEvent @event) {
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left) {
			switch (ropeState) {
                case RopeState.Hidden:
                    hook.Call("ShowHook");
                    ropeState = RopeState.Shot;
                    break;
                case RopeState.Hooked:
                    ropeState = RopeState.Retracting;
                    break;
            }
		}
	}

	private void HandleMovement(double delta) {
        Vector2 direction = Vector2.Zero;

        if (Input.IsActionPressed("ui_right")) {
            direction.X += 1;
        }
        if (Input.IsActionPressed("ui_left")) {
            direction.X -= 1;
        }
        if (Input.IsActionPressed("ui_down")) {
            direction.Y += 1;
        }
        if (Input.IsActionPressed("ui_up")) {
            direction.Y -= 1;
        }

        direction = direction.Normalized();
        Position += direction * moveSpeed * (float)delta;
    } 

	public void MakeRopeStraight() {
        Vector2 direction = (hook.GlobalPosition - player.GlobalPosition).Normalized();
        float totalDistance = player.GlobalPosition.DistanceTo(hook.GlobalPosition);
        float interval = totalDistance / (len + 1);

        for (int i = 0; i < len; i++) {
            Vector2 position = player.GlobalPosition + direction * interval * (i + 1);
            ropePieces[i].GlobalPosition = position;
        	ropePieces[i].Rotation = direction.Angle();
        }
    }

	public void ClearRope() {
        for (int i = 1; i < len; i++) {
            ropePieces[i].QueueFree();
        }
        len = 1;
        lastPiece = hook;
    }
}
