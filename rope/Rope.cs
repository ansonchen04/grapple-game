using Godot;
using System;

public partial class Rope : Node2D {
    // Called when the node enters the scene tree for the first time.
    RigidBody2D hook;
    RigidBody2D lastPiece;
    CharacterBody2D player;
    RopeState ropeState;

    const float MaxLength = 200.0f;  // max length of rope
    float moveSpeed = 300f;  // Speed of the rope movement
    const float PieceLen = 16.0f;
    bool ropeBuilt = false;
    bool hookShot = false;
    RigidBody2D[] ropePieces;  // 0 is the hook

    Vector2 playerPull;  // the pull of the rope on the player
    Vector2 mouseLoc;
    [Export] const float PullMult = 0.01f;

    public override void _Ready() {
        hook = GetNode<RigidBody2D>("Hook");
        player = GetNode<CharacterBody2D>("../Player");
        lastPiece = hook;
        ropeState = RopeState.Hidden;
        playerPull = Vector2.Zero;
        ropePieces = new RigidBody2D[(int) (MaxLength / PieceLen) + 1];
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        // Handle input for moving the rope (debug)
        // HandleMovement(delta);

        switch (ropeState) {
            case RopeState.Hidden:
                if (ropeBuilt) {
                    ClearRope();
                } else if (hookShot) {
                    HideHook();
                }
                break;
            case RopeState.Shot:
                // check if maxlength or if the rope (not hook) collides with something, if it does go to slack
                // otherwise, just keep extending the rope length
                if (!hookShot) {
                    ShootHook();
                }
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

    public override void _Input(InputEvent @event) {
        // lmb
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left) {
			switch (ropeState) {
                case RopeState.Hidden:
                    ropeState = RopeState.Shot;
                    break;
                case RopeState.Shot:
                    ropeState = RopeState.Hidden;
                    break;
                case RopeState.Hooked:
                    ropeState = RopeState.Hidden;
                    break;
            }
		}
	}

    // debug
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

    // builds the rope!
    // make sure the hook is never further than the max dist when building the rope or bad things will happen
    public void BuildRope(Vector2 hookCenter, Vector2 playerCenter) {  // to do ROPE SPAWN NO WORK
        GlobalPosition = hook.GlobalPosition;
        hook.GlobalPosition = GlobalPosition;
        float angle = (playerCenter - hookCenter).Angle();
        Vector2 dir = (playerCenter - hookCenter).Normalized();

        Vector2 hookPos = hookCenter - dir * 22;
        Vector2 playerPos = playerCenter;

        hook.Rotation = angle - (float) Math.PI / 2;

        float dist = playerPos.DistanceTo(hookPos);
        lastPiece = hook;

        int len = 0;  // ropePieces.Length doesn't give a good value for whatever reason. so i'm using this.

        // i'm not actually sure why i have to subtract 2 here. but if i don't it's not centered.
        int numPieces = (int) (dist / PieceLen) - 2;  
        for (int i = 0; i < numPieces; i++) {
            ropePieces[i] = lastPiece;
            lastPiece = (RigidBody2D) lastPiece.Call("AddNewPiece", angle); 
            len++;
        }
        ropePieces[len] = lastPiece;

        // connect to the player
        lastPiece.Call("ConnectToPlayer", player);
    }

    public void CalculateRopePull() {
        RigidBody2D iter = lastPiece;

        RigidBody2D lpParent = (RigidBody2D) lastPiece.Call("GetPieceParent");
        Vector2 lpMarkerPos = (Vector2) lastPiece.Call("GetMarkerPos");  // last piece marker pos
        Vector2 lppJointPos = (Vector2) lpParent.Call("GetJointPos");  // last piece parent joint pos

        Vector2 lppDir = (lppJointPos - lpMarkerPos).Normalized();  // direction of the parent of the past rope piece
        float totalDist = 0;
        while (lpParent is RopePiece) {  // this doesn't get the hook to rope dist, but it should be small so whatever
            totalDist += lpMarkerPos.DistanceTo(lppJointPos);
            iter = (RigidBody2D) iter.Call("GetPieceParent");
            lpParent = (RigidBody2D) iter.Call("GetPieceParent");
            lpMarkerPos = (Vector2) lastPiece.Call("GetMarkerPos");  // last piece marker pos
            lppJointPos = (Vector2) lpParent.Call("GetJointPos");  // last piece parent joint pos
        }

        float maxPullForce = 1000f;  // Maximum force that can be applied
        playerPull = lppDir * totalDist * PullMult;
        playerPull = playerPull.LimitLength(maxPullForce);
        //GD.Print(playerPull);
    }

    // clear the rope.
    public void ClearRope() {
        hook.Call("ClearJoint");

        // Iterate through the array of rope pieces
        for (int i = 1; i < ropePieces.Length; i++) {
            if (ropePieces[i] != null) {
                // Queue each rope piece for deletion
                ropePieces[i].Call("ClearJoint");
                ropePieces[i].QueueFree();
                ropePieces[i] = null;
            }
        }

        // Reset the last piece to the hook
        lastPiece = hook;

        // Reset the rope built flag
        ropeBuilt = false;

        // Reset the player pull vector
        playerPull = Vector2.Zero;

        // Reset the rope state
        ropeState = RopeState.Hidden;

        // hide the hook
        hook.Call("HideHook");

        //GD.Print("Rope cleared");
    }

    // helper function that creates a vector of length length and in direction angleInRadians
	private Vector2 CreateVector(float length, float angleInRadians) {
        float x = length * Mathf.Cos(angleInRadians);
        float y = length * Mathf.Sin(angleInRadians);

        return new Vector2(x, y);
    }

    public void ShootHook() {
        hook.Call("ShowHook");

        hook.GlobalPosition = GlobalPosition;
        GlobalPosition = (Vector2) player.Call("GetHookStartPos");

        float angle = (player.GlobalPosition - hook.GlobalPosition).Angle();
        hook.Rotation = angle - (float) Math.PI / 2;

        hook.Call("Shoot", angle);
        hookShot = true;
    }

    public void HideHook() {
        hook.Call("HideHook");
        hookShot = false;
    }

    public Vector2 GetPull() {
        return playerPull;
    }

    public void SetMouseLoc(Vector2 clickPos) {
        mouseLoc = clickPos;
    }
}
