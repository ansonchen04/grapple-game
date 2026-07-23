using Godot;
using System;

// Represents the grapple hook.
public partial class hook : RigidBody2D
{
    // Called when the node enters the scene tree.
    public override void _Ready()
    {
        HideHook();
    }

    // Hides the hook.
    public void HideHook()
    {
        Visible = false;
    }

    // Shows the hook.
    public void ShowHook()
    {
        Visible = true;
    }
}
