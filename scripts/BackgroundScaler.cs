using Godot;
using System.Linq;

// Scales background sprites to cover the entire viewport, regardless of window size.
// This ensures the background always fills the screen without stretching or leaving gaps.
public partial class BackgroundScaler : CanvasLayer
{
    // Array of background sprites to scale.
    private Sprite2D[] _backgroundSprites;

    // Called when the node enters the scene tree.
    // Initializes the list of background sprites and performs the initial scale update.
    public override void _Ready()
    {
        // Try to get all nodes in the "BackgroundSprites" group.
        _backgroundSprites = GetTree().GetNodesInGroup("BackgroundSprites").Cast<Sprite2D>().ToArray();
        
        // If no nodes are in the group, fall back to using all Sprite2D children of this CanvasLayer.
        if (_backgroundSprites.Length == 0)
        {
            _backgroundSprites = GetChildren().OfType<Sprite2D>().ToArray();
        }
        
        // Perform the initial scale calculation.
        UpdateBackgroundScale();
    }

    // Called when the node receives a notification.
    // We listen for NOTIFICATION_RESIZED (36) to update the background scale when the window is resized.
    public override void _Notification(int what)
    {
        if (what == 36) // NOTIFICATION_RESIZED
        {
            UpdateBackgroundScale();
        }
    }

    // Calculates and applies the correct scale and position for each background sprite.
    private void UpdateBackgroundScale()
    {
        // Get the current viewport size and center.
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
                    
                    // Calculate the scale needed to cover the viewport in both dimensions.
                    float scaleX = viewportSize.X / textureSize.X;
                    float scaleY = viewportSize.Y / textureSize.Y;
                    
                    // Use the larger scale to ensure the background covers the entire screen.
                    // This may result in some parts of the texture being cropped, but prevents gaps.
                    float scale = Mathf.Max(scaleX, scaleY);

                    // Apply the calculated scale.
                    sprite.Scale = new Vector2(scale, scale);
                    
                    // Center the sprite in the viewport.
                    sprite.Position = viewportCenter;
                }
            }
        }
    }
}
