using Godot;

public partial class SpinningAnchor : Node2D
{
    [Export]
    public float RotationSpeed = 1.0f; // Radians per second, positive for clockwise

    public override void _Process(double delta)
    {
        Rotation += RotationSpeed * (float)delta;
    }
}
