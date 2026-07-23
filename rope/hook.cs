using Godot;
using System;

/// <summary>
/// Represents the grapple hook.
/// </summary>
public partial class hook : RigidBody2D
{
    /// <summary>
    /// Called when the node enters the scene tree.
    /// </summary>
    public override void _Ready()
    {
        HideHook();
    }

    /// <summary>
    /// Hides the hook.
    /// </summary>
    public void HideHook()
    {
        Visible = false;
    }

    /// <summary>
    /// Shows the hook.
    /// </summary>
    public void ShowHook()
    {
        Visible = true;
    }
}
