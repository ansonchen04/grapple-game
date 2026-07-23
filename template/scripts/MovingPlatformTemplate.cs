using Godot;
using System;

/// <summary>
/// A template for moving platforms that follow a path.
/// </summary>
public partial class MovingPlatformTemplate : Node2D
{
    /// <summary>
    /// The PathFollow2D node that moves along the path.
    /// </summary>
    private PathFollow2D pathFollow;

    /// <summary>
    /// The Path2D node defining the movement path.
    /// </summary>
    private Path2D path;

    /// <summary>
    /// The speed of the platform.
    /// </summary>
    [Export]
    public int Speed = 250;

    /// <summary>
    /// The current direction of movement (1 or -1).
    /// </summary>
    private int direction = 1;

    /// <summary>
    /// Called when the node enters the scene tree.
    /// </summary>
    public override void _Ready()
    {
        pathFollow = GetNode<PathFollow2D>("PlatformPath/PathFollow2D");
        path = GetNode<Path2D>("PlatformPath");
    }

    /// <summary>
    /// Called every frame. Updates the platform's position along the path.
    /// </summary>
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
