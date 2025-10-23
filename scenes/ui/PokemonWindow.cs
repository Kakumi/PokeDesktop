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
    }

    private void Instance_MinimumSizeChanged()
    {
        var size = Instance.GetCombinedMinimumSize();
        Size = new Vector2I((int)size.X, (int)size.Y);
        Position = new Vector2I(Position.X, GetWindowDefaultPosition().Y);
    }

    public int GetWindowMaxX()
    {
        var viewW = DisplayServer.ScreenGetSize().X;

        return (int)Mathf.Floor(viewW - Instance.GetSize().Y);
    }

    public Vector2I GetWindowDefaultPosition()
    {
        var screen = DisplayServer.GetPrimaryScreen();
        var screenSize = DisplayServer.ScreenGetSize(screen);
        Rect2I usable = DisplayServer.ScreenGetUsableRect(screen);
        var taskbarHeight = screenSize.Y - (usable.Position.Y + usable.Size.Y);

        var ceilSize = Instance.GetCombinedMinimumSize().Ceil();
        var frameSize = new Vector2I((int)ceilSize.X, (int)ceilSize.Y);

        if (_defaultPosX == -1)
        {
            _defaultPosX = _rng.RandiRange(0, GetWindowMaxX());
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
