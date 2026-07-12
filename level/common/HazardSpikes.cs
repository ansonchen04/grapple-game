using Godot;

public partial class HazardSpikes : Area2D
{
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node body)
    {
        if (body is player p)
        {
            p.restart();
        }
    }
}
