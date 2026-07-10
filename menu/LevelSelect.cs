using Godot;

public partial class LevelSelect : Control
{
    public override void _Ready()
    {
        Button level1Button = GetNode<Button>("MenuContainer/Level1Button");
        Button backButton = GetNode<Button>("MenuContainer/BackButton");

        level1Button.Pressed += OnLevel1Pressed;
        backButton.Pressed += OnBackPressed;
    }

    private void OnLevel1Pressed()
    {
        GetTree().ChangeSceneToFile("res://level/level_1/level1.tscn");
    }

    private void OnBackPressed()
    {
        GetTree().ChangeSceneToFile("res://menu/main_menu.tscn");
    }
}
