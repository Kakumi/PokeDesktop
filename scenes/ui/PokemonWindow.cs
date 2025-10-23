using Godot;

public partial class PokemonWindow : Window
{
    public PokemonInstance Instance { get; private set; }

    private int _defaultPosX = -1;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    public override void _Ready()
    {
        Instance = GetNode<PokemonInstance>("View");

        Instance.MinimumSizeChanged += Instance_MinimumSizeChanged;

        CurrentScreen = GetScreen();
    }

    private void Instance_MinimumSizeChanged()
    {
        var size = Instance.GetCombinedMinimumSize();
        Size = new Vector2I((int)size.X, (int)size.Y);
        Position = new Vector2I(Position.X, GetWindowDefaultPosition().Y - Instance?.GetOffsetY() ?? 0);
    }

    //Return :
    //X : Start
    //Y : End
    public Vector2I GetWindowUsable()
    {
        Rect2I usable = DisplayServer.ScreenGetUsableRect(GetScreen());
        return new Vector2I(usable.Position.X, usable.End.X - (int)Instance.GetCombinedMinimumSize().X);
    }

    private int GetScreen()
    {
        var screen = SettingsManager.Instance.Settings.ScreenIndex;
        if (screen >= DisplayServer.GetScreenCount())
        {
            return DisplayServer.GetPrimaryScreen();
        }

        return screen;
    }

    public Vector2I GetWindowDefaultPosition()
    {
        var screen = GetScreen();
        var screenSize = DisplayServer.ScreenGetSize(screen);
        Rect2I usable = DisplayServer.ScreenGetUsableRect(screen);
        var taskbarHeight = screenSize.Y - (usable.Position.Y + usable.Size.Y);

        var ceilSize = Instance.GetCombinedMinimumSize().Ceil();
        var frameSize = new Vector2I((int)ceilSize.X, (int)ceilSize.Y);

        if (_defaultPosX == -1)
        {
            var windowUsable = GetWindowUsable();
            _defaultPosX = _rng.RandiRange(windowUsable.X, windowUsable.Y);
        }

        return new Vector2I(_defaultPosX, screenSize.Y - frameSize.Y - taskbarHeight);
    }

    public void Init(PartyPokemon pokemon)
    {
#if DEBUG
        Transparent = false;
        TransparentBg = false;
#endif
        Instance.Init(pokemon, this);

        Position = GetWindowDefaultPosition();
    }
}
