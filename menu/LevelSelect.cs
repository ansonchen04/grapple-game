using Godot;

/// <summary>
/// Handles the level selection menu.
/// </summary>
public partial class LevelSelect : Control
{
    /// <summary>
    /// Audio player for menu music.
    /// </summary>
    private AudioStreamPlayer menuMusic;

    /// <summary>
    /// Called when the node enters the scene tree.
    /// </summary>
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

    /// <summary>
    /// Loads Level 1.
    /// </summary>
    private void OnLevel1Pressed()
    {
        GetTree().ChangeSceneToFile("res://level/level_1/level1.tscn");
    }

    /// <summary>
    /// Loads Level 2.
    /// </summary>
    private void OnLevel2Pressed()
    {
        GetTree().ChangeSceneToFile("res://level/level_2/level2.tscn");
    }

    /// <summary>
    /// Loads Level 3.
    /// </summary>
    private void OnLevel3Pressed()
    {
        GetTree().ChangeSceneToFile("res://level/level_3/level3.tscn");
    }

    /// <summary>
    /// Returns to the main menu.
    /// </summary>
    private void OnBackPressed()
    {
        GetTree().ChangeSceneToFile("res://menu/main_menu.tscn");
    }
}
