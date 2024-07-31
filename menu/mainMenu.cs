using Godot;
using System;

public partial class mainMenu : Control
{	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var config = new ConfigFile();
		var setting_data = new Godot.Collections.Dictionary();
		// Load data from a file.
		Error err = config.Load("res://data//settings.cfg");
		// If the file didn't load, create a default config.
		if (err != Error.Ok) {
			GD.Print("Errored out");
			DefaultConfig(config);
		}
		//Attaches an event to start the game 
		Button startButton = GetNode<Button>("MarginContainer/StartContainer/ButtonVBox/Start");
		startButton.Pressed += Start;
		//Attaches an event to quit the game when the quit button is pressed
		Button quitButton = GetNode<Button>("MarginContainer/StartContainer/ButtonVBox/Quit");
		quitButton.Pressed += Quit;
		//TODO Add a resume button
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	private void Quit(){
		GD.Print("Shutting down");
		GetTree().Quit();
	}
	private void Start(){
		GD.Print("Starting level 1");
		GetTree().ChangeSceneToFile("res://level/level1/Level1.tscn");
	}
	private void DefaultConfig(ConfigFile config){
		config.Save("res://data//settings.cfg");
	}
}
