using Godot;
using System;

public partial class Level3 : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var music = GetNode<AudioStreamPlayer>("Music");
		music.Play();
		music.Finished += () => music.Play();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
