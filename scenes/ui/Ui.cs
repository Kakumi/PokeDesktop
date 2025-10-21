using Godot;

public partial class Ui : Control
{
    private TextureButton _appButton;


    public override void _Ready()
    {
        _appButton = GetNode<TextureButton>("AppButton");

        _appButton.Pressed += _appButton_Pressed;
    }

    private void _appButton_Pressed()
    {
        GD.Print("press");
    }

    public Vector2[] GetRegion(Vector2I windowPosition)
    {
        Vector2 size = _appButton.Size;
        if (size.X <= 0 || size.Y <= 0)
        {
            size = _appButton.TextureNormal?.GetSize() ?? new Vector2(1, 1);
        }

        return [
            _appButton.GlobalPosition,
            _appButton.GlobalPosition + new Vector2(size.X, 0),
            _appButton.GlobalPosition + size,
            _appButton.GlobalPosition + new Vector2(0, size.Y),
        ];
    }
}
