using Godot;
using System;

public partial class PathPlatform : Node2D
{
	private PathFollow2D pathFollow;
	[Export]
	public int Speed = 500;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		pathFollow = GetNode<PathFollow2D>("PlatformPath/PathFollow2D");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		pathFollow.Progress += (float)(Speed*delta);
	}
}