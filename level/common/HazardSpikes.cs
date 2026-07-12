using Godot;

public partial class HazardSpikes : Area2D
{
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node body)
    {
        GD.Print($"HazardSpikes: Body entered: {body.Name}");
        if (body is player p)
        {
            GD.Print("HazardSpikes: Player hit! Restarting.");
            p.rope.CancelRope();
            p.restart();
        }
    }
}
