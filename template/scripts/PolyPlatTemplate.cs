using Godot;
using System;

public partial class PolyPlatTemplate : Polygon2D
{
	// Called when the node enters the scene tree for the first time.
    //NOTE THIS IS THE SAME SCRIPT FOR ALL PLATFORMS, CHANGING THIS WILL CHANGE IT FOR EVERY PLATFORM USED FROM THE TEMPLATE
	public override void _Ready()
	{
		//Creates a collision poly instance
		var collisionPoly = new CollisionPolygon2D();
		//Sets the position and its shape to the array of vectors
        collisionPoly.Polygon = this.Polygon;
		//Sets the position offset to that of the node
        collisionPoly.Position = this.Position;
        GetParent().CallDeferred("add_child", collisionPoly);
	}
}
