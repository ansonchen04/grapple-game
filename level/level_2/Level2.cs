using Godot;
using System;

// Level 2 scene controller.
public partial class Level2 : Node2D
{
	// Called when the node enters the scene tree.
	public override void _Ready()
	{
		var music = GetNode<AudioStreamPlayer>("Music");
		music.Play();
		music.Finished += () => music.Play();
	}

	// Called every frame.
	public override void _Process(double delta)
	{
	}
}
