using Godot;

// Represents the level completion flag.
public partial class Flag : Area2D
{
    // Called when the node enters the scene tree.
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    // Called when a body enters the area.
    private void OnBodyEntered(Node body)
    {
        if (body is CharacterBody2D player)
        {
            CallDeferred(nameof(ChangeToLevelComplete));
        }
    }

    // Changes the scene to the level complete menu.
    private void ChangeToLevelComplete()
    {
        GetTree().ChangeSceneToFile("res://menu/LevelComplete.tscn");
    }
}
