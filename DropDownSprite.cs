using Godot;
using System;

public partial class DropDownSprite : Polygon2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var poly = new CollisionPolygon2D();
        poly.Polygon = this.Polygon;
        poly.Position = this.Position;
		poly.OneWayCollision = true;
        GetParent().CallDeferred("add_child", poly);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
