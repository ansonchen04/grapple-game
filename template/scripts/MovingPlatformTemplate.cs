using Godot;
using System;

public partial class MovingPlatformTemplate : Node2D
{
	private PathFollow2D pathFollow;
	private Path2D path;
	[Export]
	public int Speed = 500;
	private int direction = 1;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		pathFollow = GetNode<PathFollow2D>("PlatformPath/PathFollow2D");
		path = GetNode<Path2D>("PlatformPath");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		float length = path.Curve.GetBakedLength();
		float progress = pathFollow.Progress;
		
		pathFollow.Progress += (float)(Speed * delta * direction);
		
		if (pathFollow.Progress >= length) {
			pathFollow.Progress = length;
			direction = -1;
		} else if (pathFollow.Progress <= 0) {
			pathFollow.Progress = 0;
			direction = 1;
		}
	}
}
