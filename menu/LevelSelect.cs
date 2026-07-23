using Godot;

// Handles the level selection menu.
public partial class LevelSelect : Control
{
    // Audio player for menu music.
    private AudioStreamPlayer menuMusic;

    // Called when the node enters the scene tree.
    public override void _Ready()
    {
        Button level1Button = GetNode<Button>("MenuContainer/Level1Button");
        Button level2Button = GetNode<Button>("MenuContainer/Level2Button");
        Button backButton = GetNode<Button>("MenuContainer/BackButton");

        level1Button.Pressed += OnLevel1Pressed;
        level2Button.Pressed += OnLevel2Pressed;
        Button level3Button = GetNode<Button>("MenuContainer/Level3Button");
        level3Button.Pressed += OnLevel3Pressed;
        backButton.Pressed += OnBackPressed;

        menuMusic = GetNode<AudioStreamPlayer>("MenuMusic");
        menuMusic.Play();
    }

    // Loads Level 1.
    private void OnLevel1Pressed()
    {
        GetTree().ChangeSceneToFile("res://level/level_1/level1.tscn");
    }

    // Loads Level 2.
    private void OnLevel2Pressed()
    {
        GetTree().ChangeSceneToFile("res://level/level_2/level2.tscn");
    }

    // Loads Level 3.
    private void OnLevel3Pressed()
    {
        GetTree().ChangeSceneToFile("res://level/level_3/level3.tscn");
    }

    // Returns to the main menu.
    private void OnBackPressed()
    {
        GetTree().ChangeSceneToFile("res://menu/main_menu.tscn");
    }
}
