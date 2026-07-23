using Godot;

/// <summary>
/// Represents the level completion flag.
/// </summary>
public partial class Flag : Area2D
{
    /// <summary>
    /// Called when the node enters the scene tree.
    /// </summary>
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    /// <summary>
    /// Called when a body enters the area.
    /// </summary>
    private void OnBodyEntered(Node body)
    {
        if (body is CharacterBody2D player)
        {
            CallDeferred(nameof(ChangeToLevelComplete));
        }
    }

    /// <summary>
    /// Changes the scene to the level complete menu.
    /// </summary>
    private void ChangeToLevelComplete()
    {
        GetTree().ChangeSceneToFile("res://menu/LevelComplete.tscn");
    }
}
