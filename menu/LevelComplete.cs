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
        string levelPath = player.CurrentLevelPath;
        GD.Print($"LevelComplete: Attempting to replay: {levelPath}");
        if (!string.IsNullOrEmpty(levelPath))
        {
            GetTree().ChangeSceneToFile(levelPath);
        }
        else
        {
            GD.PrintErr("LevelComplete: No level path found, falling back to Level 1");
            GetTree().ChangeSceneToFile("res://level/level_1/level1.tscn");
        }
    }
}
