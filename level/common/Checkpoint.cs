using Godot;

public partial class Checkpoint : Area2D
{
    private Sprite2D sprite;
    private bool isActivated = false;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        sprite = GetNode<Sprite2D>("Sprite2D");
        // Initially show as inactive (grayed out or different color)
        sprite.Modulate = new Color(0.5f, 0.5f, 0.5f, 1.0f);
    }

    private void OnBodyEntered(Node body)
    {
        if (body is player p && !isActivated)
        {
            isActivated = true;
            player.LastCheckpointPosition = GlobalPosition;
            
            // Visual feedback: change to active state (bright green)
            sprite.Modulate = new Color(0.0f, 1.0f, 0.0f, 1.0f);
            
            GD.Print($"Checkpoint activated at {GlobalPosition}");
        }
    }
}
