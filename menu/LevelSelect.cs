using Godot;

public partial class LevelSelect : Control
{
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
    }

    private void OnLevel1Pressed()
    {
        GetTree().ChangeSceneToFile("res://level/level_1/level1.tscn");
    }

    private void OnLevel2Pressed()
    {
        GetTree().ChangeSceneToFile("res://level/level_2/level2.tscn");
    }

    private void OnLevel3Pressed()
    {
        GetTree().ChangeSceneToFile("res://level/level_3/level3.tscn");
    }

    private void OnBackPressed()
    {
        GetTree().ChangeSceneToFile("res://menu/main_menu.tscn");
    }
}
