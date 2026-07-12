using Godot;

public partial class LevelComplete : Control
{
    private string currentLevelPath = "res://level/level_1/level1.tscn";

    public override void _Ready()
    {
        Button menuButton = GetNode<Button>("MenuContainer/MenuButton");
        Button replayButton = GetNode<Button>("MenuContainer/ReplayButton");

        menuButton.Pressed += OnMenuPressed;
        replayButton.Pressed += OnReplayPressed;

        // Capture the current scene path so we can replay it
        currentLevelPath = GetTree().CurrentScene.SceneFilePath;
    }

    private void OnMenuPressed()
    {
        GetTree().ChangeSceneToFile("res://menu/main_menu.tscn");
    }

    private void OnReplayPressed()
    {
        if (!string.IsNullOrEmpty(currentLevelPath))
        {
            GetTree().ChangeSceneToFile(currentLevelPath);
        }
        else
        {
            GetTree().ChangeSceneToFile("res://level/level_1/level1.tscn");
        }
    }
}
