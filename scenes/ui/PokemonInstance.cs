using Godot;
using System.Linq;

public partial class PokemonInstance : VBoxContainer
{
    [Export] public PokemonMovement[] Movements;

    public TextureButton EmojiButton { get; private set; }
    public Label PokemonName { get; private set; }
    public TextureRect PokemonSprite { get; private set; }

    private MovementType _movementType = MovementType.None;
    private PokemonSpriteCache _spriteCache;
    private EmotionHandler _emotionHandler;
    private BaseMovement _movement;

    public override void _Ready()
    {
        EmojiButton = GetNodeOrNull<TextureButton>("EmojiButton");
        PokemonName = GetNodeOrNull<Label>("PokemonName");
        PokemonSprite = GetNodeOrNull<TextureRect>("PokemonSprite");

        _spriteCache = GetNodeOrNull<PokemonSpriteCache>("PokemonSpriteCache");
        _emotionHandler = GetNodeOrNull<EmotionHandler>("EmotionHandler");

        _spriteCache.TextureReady += _spriteCache_TextureReady;
        _spriteCache.TextureFailed += _spriteCache_TextureFailed;
    }

    private void _spriteCache_TextureFailed(string error)
    {
        Logger.Instance.Error(error);
    }

    private void _spriteCache_TextureReady(Texture2D texture)
    {
        PokemonSprite.Texture = texture;
    }

    public void Init(PartyPokemon pokemon, PokemonWindow window)
    {
        _emotionHandler.Init(pokemon);
        _spriteCache.LoadOrDownloadTexture(pokemon.Pokemon, SettingsManager.Instance.Settings.AnimatedSprites);

        if (SettingsManager.Instance.Settings.ShowName)
        {
            PokemonName.Text = pokemon.Pokemon.GetName();
            PokemonName.Visible = true;
        }
        else
        {
            PokemonName.Visible = false;
        }

        _movementType = pokemon.Pokemon.GetMovementType();

        if (_movement != null)
        {
            _movement.Free();
        }

        var useSmartMove = SettingsManager.Instance.Settings.SmartMove;
        var moveInfo = Movements.FirstOrDefault(x => x.Type == (useSmartMove ? _movementType : MovementType.Walk));
        if (moveInfo != null)
        {
            _movement = moveInfo.PackedScene.Instantiate<BaseMovement>();
            AddChild(_movement);

            _movement.Init(pokemon.Pokemon, window, this);
        }
    }
}
