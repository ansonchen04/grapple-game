using Godot;

public partial class Checkpoint : Area2D
{
    private ColorRect visual;
    private bool isActivated = false;
    private AudioStreamPlayer checkpointSFX;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        visual = GetNode<ColorRect>("Visual");
        checkpointSFX = GetNode<AudioStreamPlayer>("CheckpointSFX");
        // Initially show as inactive (yellow/orange like the flag)
        visual.Color = new Color(1.0f, 0.8f, 0.0f, 1.0f);
    }

    private void OnBodyEntered(Node body)
    {
        if (body is player p && !isActivated)
        {
            isActivated = true;
            player.LastCheckpointPosition = GlobalPosition;
            
            // Visual feedback: change to active state (bright green)
            visual.Color = new Color(0.0f, 1.0f, 0.0f, 1.0f);
            
            checkpointSFX.Play();
            
            GD.Print($"Checkpoint activated at {GlobalPosition}");
        }
    }
}
