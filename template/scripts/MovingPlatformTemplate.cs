using Godot;
using System;

// A template for moving platforms that follow a path.
public partial class MovingPlatformTemplate : Node2D
{
    // The PathFollow2D node that moves along the path.
    private PathFollow2D pathFollow;

    // The Path2D node defining the movement path.
    private Path2D path;

    // The speed of the platform.
    [Export]
    public int Speed = 250;

    // The current direction of movement (1 or -1).
    private int direction = 1;

    // Called when the node enters the scene tree.
    public override void _Ready()
    {
        pathFollow = GetNode<PathFollow2D>("PlatformPath/PathFollow2D");
        path = GetNode<Path2D>("PlatformPath");
    }

    // Called every frame. Updates the platform's position along the path.
    public override void _Process(double delta)
    {
        float length = path.Curve.GetBakedLength();
        float progress = pathFollow.Progress;

        pathFollow.Progress += (float)(Speed * delta * direction);

        if (pathFollow.Progress >= length)
        {
            pathFollow.Progress = length;
            direction = -1;
        }
        else if (pathFollow.Progress <= 0)
        {
            pathFollow.Progress = 0;
            direction = 1;
        }
    }
}
