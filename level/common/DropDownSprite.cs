using Godot;
using System;

public partial class DropDownSprite : Polygon2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var collisionPoly = new CollisionPolygon2D();
        collisionPoly.Polygon = this.Polygon;
        collisionPoly.Position = this.Position;
		collisionPoly.OneWayCollision = true;		
        GetParent().CallDeferred("add_child", collisionPoly);
	
	}

}
