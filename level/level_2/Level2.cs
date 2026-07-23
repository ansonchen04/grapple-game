using Godot;
using System;

/// <summary>
/// Level 2 scene controller.
/// </summary>
public partial class Level2 : Node2D
{
    /// <summary>
    /// Called when the node enters the scene tree.
    /// </summary>
    public override void _Ready()
    {
        var music = GetNode<AudioStreamPlayer>("Music");
        music.Play();
        music.Finished += () => music.Play();
    }

    /// <summary>
    /// Called every frame.
    /// </summary>
    public override void _Process(double delta)
    {
    }
}
