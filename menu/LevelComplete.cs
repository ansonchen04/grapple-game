using Godot;

public partial class LevelComplete : Control
{
    public override void _Ready()
    {
        Button menuButton = GetNode<Button>("MenuContainer/MenuButton");
        Button replayButton = GetNode<Button>("MenuContainer/ReplayButton");

        menuButton.Pressed += OnMenuPressed;
        replayButton.Pressed += OnReplayPressed;
    }

    private void OnMenuPressed()
    {
        GetTree().ChangeSceneToFile("res://menu/main_menu.tscn");
    }

    private void OnReplayPressed()
    {
        GetTree().ChangeSceneToFile("res://level/level_1/level1.tscn");
    }
}
