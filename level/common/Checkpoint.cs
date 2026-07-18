using Godot;

public partial class Checkpoint : Area2D
{
    private Sprite2D visual;
    private bool isActivated = false;
    private AudioStreamPlayer checkpointSFX;
    private Texture2D eatenBananaTexture;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        visual = GetNode<Sprite2D>("Visual");
        checkpointSFX = GetNode<AudioStreamPlayer>("CheckpointSFX");
        eatenBananaTexture = GD.Load<Texture2D>("res://sprites/eatenbanana.png");
    }

    private void OnBodyEntered(Node body)
    {
        if (body is player p && !isActivated)
        {
            isActivated = true;
            player.LastCheckpointPosition = GlobalPosition;
            
            // Visual feedback: switch to eaten banana sprite
            if (visual != null && eatenBananaTexture != null)
            {
                visual.Texture = eatenBananaTexture;
            }
            
            checkpointSFX.Play();
            
            GD.Print($"Checkpoint activated at {GlobalPosition}");
        }
    }
}
