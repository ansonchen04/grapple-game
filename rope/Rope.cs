using Godot;
using System;

public partial class Rope : Node2D {
    // Called when the node enters the scene tree for the first time.
    RigidBody2D hook;
    RigidBody2D lastPiece;
    RopeState ropeState;
    int len = 0;
    const int MaxLength = 10;  // max num links in the rope
    float distToHook = 0;
    float moveSpeed = 300f;  // Speed of the rope movement

    public override void _Ready() {
        hook = GetNode<RigidBody2D>("Hook");
        lastPiece = hook;
        ropeState = RopeState.Hidden;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        // Handle input for moving the rope
        HandleMovement(delta);

        switch (ropeState) {
            case RopeState.Hidden:
                ropeState = RopeState.Shot;  // for testing!
                break;
            case RopeState.Shot:
                // check if maxlength or if the rope (not hook) collides with something, if it does go to slack
                // otherwise, just keep extending the rope length
                if (len < MaxLength) {  // add the rope collision thing later
                    // todo later: update positions of rope pieces to be a straight line

                    // for now, i'll just call it once - but need to figure out the distance from hook 
                    // and use that to determine number of times to call this
                    lastPiece.Call("SetId", len);
                    lastPiece = (RigidBody2D) lastPiece.Call("AddNewPiece");
                    len++;
                } else {  
                    ropeState = RopeState.Slack;
                }
                break;
            case RopeState.Hooked:
                // stop growing the rope. lock the hook in place.
                break;
            case RopeState.Retracting:
                // while holding right click and is hooked, retract
                // can also experiment with just clicking right click and automatically retracting the whole thing
                // also play w diff speeds
                break;
            case RopeState.Slack:
                // gravity starts working on the whole rope + hook. stop extending the rope.
                // if the hook hits something switch states to hooked
                break;
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
        // make the rope straight from the hook to the player
    }

    public void ClearRope() {
        // clear the rope.
    }
}
