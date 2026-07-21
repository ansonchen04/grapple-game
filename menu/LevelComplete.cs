using Godot;

public partial class LevelComplete : Control
{
	private Button nextLevelButton;
	private AudioStreamPlayer victoryMusic;

	public override void _Ready()
	{
		Button menuButton = GetNode<Button>("MenuContainer/MenuButton");
		Button replayButton = GetNode<Button>("MenuContainer/ReplayButton");
		nextLevelButton = GetNode<Button>("MenuContainer/NextLevelButton");

		menuButton.Pressed += OnMenuPressed;
		replayButton.Pressed += OnReplayPressed;
		nextLevelButton.Pressed += OnNextLevelPressed;

		victoryMusic = GetNode<AudioStreamPlayer>("VictoryMusic");
		victoryMusic.Play();

		// Determine if there is a next level
		string currentLevel = player.CurrentLevelPath;
		if (currentLevel.Contains("level_1"))
		{
			nextLevelButton.Disabled = false;
			nextLevelButton.Text = "Level 2";
		}
		else if (currentLevel.Contains("level_2"))
		{
			nextLevelButton.Disabled = false;
			nextLevelButton.Text = "Level 3";
		}
		else
		{
			// No more levels for now
			nextLevelButton.Disabled = true;
			nextLevelButton.Text = "No More Levels";
		}
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

	private void OnNextLevelPressed()
	{
		string currentLevel = player.CurrentLevelPath;
		if (currentLevel.Contains("level_1"))
		{
			GetTree().ChangeSceneToFile("res://level/level_2/level2.tscn");
		}
		else if (currentLevel.Contains("level_2"))
		{
			GetTree().ChangeSceneToFile("res://level/level_3/level3.tscn");
		}
		else
		{
			GD.Print("No more levels available.");
		}
	}
}
