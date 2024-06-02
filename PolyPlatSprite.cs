using Godot;
using System;

public partial class PolyPlatSprite : Polygon2D
{
	// Called when the node enters the scene tree for the first time.
    //NOTE THIS IS THE SAME SCRIPT FOR ALL PLATFORMS, CHANGING THIS WILL CHANGE IT FOR EVERY PLATFORM USED FROM THE TEMPLATE
	public override void _Ready()
	{
		var poly = new CollisionPolygon2D();
        poly.Polygon = this.Polygon;
        poly.Position = this.Position;
        GetParent().CallDeferred("add_child", poly);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
