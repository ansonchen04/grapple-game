using Godot;
using System.Linq;

public partial class BackgroundScaler : CanvasLayer
{
    [Export] private float baseWidth = 1920.0f;
    [Export] private float baseHeight = 1080.0f;

    private Sprite2D[] _backgroundSprites;

    public override void _Ready()
    {
        _backgroundSprites = GetTree().GetNodesInGroup("BackgroundSprites").Cast<Sprite2D>().ToArray();
        if (_backgroundSprites.Length == 0)
        {
            // Fallback: get all Sprite2D children if no group is set
            _backgroundSprites = GetChildren().OfType<Sprite2D>().ToArray();
        }
        UpdateBackgroundScale();
    }

    public override void _Notification(int what)
    {
        if (what == 36) // NOTIFICATION_RESIZED
        {
            UpdateBackgroundScale();
        }
    }

    private void UpdateBackgroundScale()
    {
        var viewportSize = GetViewport().Size;
        float scaleX = viewportSize.X / baseWidth;
        float scaleY = viewportSize.Y / baseHeight;
        
        // Use the larger scale to ensure the background covers the screen
        float scale = Mathf.Max(scaleX, scaleY);

        foreach (var sprite in _backgroundSprites)
        {
            if (sprite != null)
            {
                sprite.Scale = new Vector2(scale, scale);
            }
        }
    }
}
