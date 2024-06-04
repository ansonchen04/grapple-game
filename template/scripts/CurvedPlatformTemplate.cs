using Godot;
using System;

public partial class CurvedPlatformTemplate : Node2D
{
    public override void _Ready()
    {
        // Get the Curve2D node from the Path2D
        var curve = GetNode<Path2D>("Path2D").Curve;
        
        // Get the baked points from the curve
        var polygon = curve.GetBakedPoints();
        
        // Assign the points to Polygon2D
        GetNode<Polygon2D>("Polygon2D").Polygon = polygon;
        
        // Assign the points to Line2D
        GetNode<Line2D>("Line2D").Points = polygon;
        
        // Create a new CollisionPolygon2D
        var collisionPoly = new CollisionPolygon2D();
		collisionPoly.Polygon = polygon;

        // Add the CollisionPolygon2D as a child of this node
        AddChild(collisionPoly);
    }
}
