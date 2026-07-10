using Godot;

public partial class Flag : Area2D
{
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node body)
    {
        if (body is CharacterBody2D player)
        {
            GetTree().ChangeSceneToFile("res://menu/LevelComplete.tscn");
        }
    }
}
