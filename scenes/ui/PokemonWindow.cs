using Godot;
using PKHeX.Core;

public partial class PokemonPopup : Window
{
    public PokemonFrame PokemonFrame { get; private set; }

    public override void _Ready()
    {
        PokemonFrame = GetNode<PokemonFrame>("PokemonFrame");
    }

    public void Init(PKM pokemon)
    {
        var screen = DisplayServer.GetPrimaryScreen();
        var screenSize = DisplayServer.ScreenGetSize(screen);
        Rect2I usable = DisplayServer.ScreenGetUsableRect(screen);
        var taskbarHeight = screenSize.Y - (usable.Position.Y + usable.Size.Y);

        Size = new Vector2I(screenSize.X, Size.Y);

        PokemonFrame.Init(pokemon);

        Vector2I windowPosition = new Vector2I(0, screenSize.Y - Size.Y - taskbarHeight);

        Position = windowPosition;
    }
}
