using Godot;
using System;

public partial class Rope : Node2D {
    // Called when the node enters the scene tree for the first time.
    RigidBody2D hook;
    RigidBody2D lastPiece;
    CharacterBody2D player;
    RopeState ropeState;

    int len = 0;
    const float MaxLength = 200.0f;  // max num links in the rope
    float distToHook = 0;
    float moveSpeed = 300f;  // Speed of the rope movement
    const float PieceLen = 16.0f;
    bool ropeBuilt = false;

    Vector2 playerPull;  // the pull of the rope on the player
    const float PullMult = 1;

    public override void _Ready() {
        hook = GetNode<RigidBody2D>("Hook");
        player = GetNode<CharacterBody2D>("../Player");
        lastPiece = hook;
        ropeState = RopeState.Hidden;
        playerPull = Vector2.Zero;
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
                if (player.GlobalPosition.DistanceTo(hook.GlobalPosition) > MaxLength) {  
                    ropeState = RopeState.Hooked;
                } 
                break;
            case RopeState.Hooked:
                // summon the rope. lock the hook in place.
                if (!ropeBuilt) {
                    BuildRope(hook.GlobalPosition, player.GlobalPosition);
                    ropeBuilt = true;
                }
                CalculateRopePull();
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

    public void BuildRope(Vector2 hookCenter, Vector2 playerCenter) {
        float angle = (playerCenter - hookCenter).Angle();
        Vector2 dir = (playerCenter - hookCenter).Normalized();

        Vector2 hookPos = hookCenter - dir * 22;
        Vector2 playerPos = playerCenter;

        hook.Rotation = angle - (float) Math.PI / 2;
        float dist = playerPos.DistanceTo(hookPos);

        // i'm not actually sure why i have to subtract 2 here. but if i don't it's not centered.
        int numPieces = (int) (dist / PieceLen) - 2;  
        for (int i = 0; i < numPieces; i++) {
            lastPiece = (RigidBody2D) lastPiece.Call("AddNewPiece", angle);  // get the position working???
        }

        // connect to the player
        lastPiece.Call("ConnectToPlayer", player);
    }

    public void CalculateRopePull() {
        RigidBody2D lpParent = (RigidBody2D) lastPiece.Call("GetPieceParent");
        Vector2 lpMarkerPos = (Vector2) lastPiece.Call("GetMarkerPos");  // last piece parker pos
        Vector2 lppJointPos = (Vector2) lpParent.Call("GetJointPos");  // last piece parent joint pos

        float jmDist = lpMarkerPos.DistanceTo(lppJointPos);  // distance between pieces, basically
        Vector2 lppDir = (lppJointPos - lpMarkerPos).Normalized();

        playerPull = lppDir * jmDist * PullMult;
        //GD.Print(playerPull);
    }

    public void ClearRope() {
        // clear the rope.
    }

    public Vector2 GetPull() {
        return playerPull;
    }
}
