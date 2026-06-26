using Godot;
using System;

public partial class hook : RigidBody2D
{
    public override void _Ready() {
        HideHook();
    }

    public void HideHook() {
        Visible = false;
    }

    public void ShowHook() {
        Visible = true;
    }
}
