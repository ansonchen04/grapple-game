using Godot;

public partial class MainMenu : Control
{
    public override void _Ready()
    {
        Button playButton = GetNode<Button>("MenuContainer/PlayButton");
        Button quitButton = GetNode<Button>("MenuContainer/QuitButton");

        playButton.Pressed += OnPlayPressed;
        quitButton.Pressed += OnQuitPressed;
    }

    private void OnPlayPressed()
    {
        GetTree().ChangeSceneToFile("res://menu/LevelSelect.tscn");
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
    }
}
