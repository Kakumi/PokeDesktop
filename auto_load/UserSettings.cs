using Godot;

public partial class UserSettings : Node
{
    [Export] public PackedScene AppMenu;

    public override void _Ready()
    {
        var window = AppMenu.Instantiate<Window>();
        AddChild(window);

        window.CloseRequested += () =>
        {
            window.QueueFree();
        };
    }
}
