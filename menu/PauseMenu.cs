using Godot;

public partial class PauseMenu : CanvasLayer
{
	public override void _Ready()
	{
		// ProcessMode is set in the .tscn file, but we can ensure it here if needed.
		// However, since we changed the node type to CanvasLayer, we don't need to set ProcessMode in code 
		// if it's already set in the scene, but keeping it doesn't hurt.
		ProcessMode = ProcessModeEnum.Always;
		
		// Ensure the menu is hidden when the scene loads
		Visible = false;
		
		// Connect buttons to their respective handlers
		GetNode<Button>("VBoxContainer/ResumeButton").Pressed += OnResumePressed;
		GetNode<Button>("VBoxContainer/RestartButton").Pressed += OnRestartPressed;
		GetNode<Button>("VBoxContainer/QuitButton").Pressed += OnQuitPressed;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		// Check for the "ui_cancel" action (typically Escape key)
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && Input.IsActionPressed("ui_cancel"))
		{
			TogglePause();
		}
	}

	private void TogglePause()
	{
		bool isPaused = GetTree().Paused;
		GetTree().Paused = !isPaused;
		Visible = !isPaused;
	}

	private void OnResumePressed()
	{
		GetTree().Paused = false;
		Visible = false;
	}

	private void OnRestartPressed()
	{
		GetTree().Paused = false;
		Visible = false;
		// Reload the current scene
		GetTree().ReloadCurrentScene();
	}

	private void OnQuitPressed()
	{
		GetTree().Paused = false;
		Visible = false;
		// Load the main menu scene
		GetTree().ChangeSceneToFile("res://menu/main_menu.tscn");
	}
}
