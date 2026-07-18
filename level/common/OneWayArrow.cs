using Godot;

public partial class OneWayArrow : Node2D
{
    private const float ArrowSize = 20.0f;
    private const Color ArrowColor = new Color(1.0f, 1.0f, 1.0f, 0.8f);

    public override void _Ready()
    {
        Visible = true;
    }

    public override void _Draw()
    {
        // Draw an upward-pointing arrow
        // Center of the arrow is at (0, 0)
        
        // Arrow shaft
        DrawLine(new Vector2(0, ArrowSize), new Vector2(0, -ArrowSize * 0.5f), ArrowColor, 4.0f);
        
        // Arrow head (left side)
        DrawLine(new Vector2(-ArrowSize * 0.5f, ArrowSize * 0.2f), new Vector2(0, -ArrowSize * 0.5f), ArrowColor, 4.0f);
        
        // Arrow head (right side)
        DrawLine(new Vector2(ArrowSize * 0.5f, ArrowSize * 0.2f), new Vector2(0, -ArrowSize * 0.5f), ArrowColor, 4.0f);
    }
}
