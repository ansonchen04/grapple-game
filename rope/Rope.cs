using Godot;
using System;

public partial class Rope : Node2D {
    CharacterBody2D player;
    public RopeState ropeState;

    const float MaxLength = 500.0f;
    
    // Placeholders for Verlet integration (Chunk 2)
    Vector2[] positions;
    Vector2[] previousPositions;
    const int MaxSegments = 30;

    public override void _Ready() {
        player = GetNode<CharacterBody2D>("../Player");
        ropeState = RopeState.Hidden;
        
        // Initialize Verlet arrays (will be populated in Chunk 2)
        positions = new Vector2[MaxSegments + 1];
        previousPositions = new Vector2[MaxSegments + 1];
    }

    public override void _Process(double delta) {
        switch (ropeState) {
            case RopeState.Hidden:
                // Reset state when hidden
                break;
            case RopeState.Shot:
                // Raycast aiming will go here in Chunk 3
                break;
            case RopeState.Hooked:
                // Verlet simulation & player attachment in Chunks 2 & 4
                break;
            case RopeState.Retracting:
                // Retract logic in Chunk 5
                break;
            case RopeState.Slack:
                // Slack logic in Chunk 5
                break;
        }
    }

    public override void _Input(InputEvent @event) {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left) {
            switch (ropeState) {
                case RopeState.Hidden:
                    ropeState = RopeState.Shot;
                    break;
                case RopeState.Shot:
                case RopeState.Hooked:
                case RopeState.Slack:
                    ropeState = RopeState.Hidden;
                    break;
            }
            GD.Print($"Rope State changed to: {ropeState}");
        }
    }
}
