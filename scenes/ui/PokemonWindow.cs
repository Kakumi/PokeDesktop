using Godot;
using PKHeX.Core;

public partial class PokemonWindow : Window
{
    public PokemonFrame PokemonFrame { get; private set; }

    private int _defaultPosX = 0;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    public override void _Ready()
    {
        PokemonFrame = GetNode<PokemonFrame>("PokemonFrame");
    }

    public int GetWindowMaxX()
    {
        var viewW = DisplayServer.ScreenGetSize().X;

        return (int)Mathf.Floor(viewW - PokemonFrame.GetSize().Y);
    }

    public Vector2I GetWindowDefaultPosition()
    {
        var screen = DisplayServer.GetPrimaryScreen();
        var screenSize = DisplayServer.ScreenGetSize(screen);
        Rect2I usable = DisplayServer.ScreenGetUsableRect(screen);
        var taskbarHeight = screenSize.Y - (usable.Position.Y + usable.Size.Y);
        var ceilSize = PokemonFrame.GetSize().Ceil();
        var frameSize = new Vector2I((int)ceilSize.X, (int)ceilSize.Y);

        return new Vector2I(_defaultPosX, screenSize.Y - frameSize.Y - taskbarHeight);
    }

    public void Init(PKM pokemon)
    {
        var screen = DisplayServer.GetPrimaryScreen();
        var screenSize = DisplayServer.ScreenGetSize(screen);
        Rect2I usable = DisplayServer.ScreenGetUsableRect(screen);
        var taskbarHeight = screenSize.Y - (usable.Position.Y + usable.Size.Y);

#if DEBUG
        Transparent = false;
        TransparentBg = false;
#endif
        PokemonFrame.Init(pokemon, this);

        var ceilSize = PokemonFrame.GetSize().Ceil();
        Size = new Vector2I((int)ceilSize.X, (int)ceilSize.Y); //new Vector2I(screenSize.X, Size.Y);
        //var minSize = GetNode<VBoxContainer>("View").GetCombinedMinimumSize();
        //Size = new Vector2I((int)minSize.X, (int)minSize.Y);

        _defaultPosX = _rng.RandiRange(0, GetWindowMaxX());

        Position = GetWindowDefaultPosition();
    }
}
