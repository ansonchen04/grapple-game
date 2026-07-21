using Godot;
using System.Linq;

public partial class BackgroundScaler : CanvasLayer
{
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
        var viewportSize = GetViewport().GetVisibleRect().Size;
        var viewportCenter = viewportSize / 2.0f;

        foreach (var sprite in _backgroundSprites)
        {
            if (sprite != null)
            {
                var texture = sprite.Texture;
                if (texture != null)
                {
                    var textureSize = texture.GetSize();
                    
                    float scaleX = viewportSize.X / textureSize.X;
                    float scaleY = viewportSize.Y / textureSize.Y;
                    
                    // Use the larger scale to ensure the background covers the screen
                    float scale = Mathf.Max(scaleX, scaleY);

                    sprite.Scale = new Vector2(scale, scale);
                    // Center the sprite in the viewport
                    sprite.Position = viewportCenter;
                }
            }
        }
    }
}
