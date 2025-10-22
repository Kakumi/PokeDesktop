using Godot;
using PKHeX.Core;
using System;
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
    private Random _rng;
    private BaseMovement _movement;

    public override void _Ready()
    {
        EmojiButton = GetNodeOrNull<TextureButton>("EmojiButton");
        PokemonName = GetNodeOrNull<Label>("PokemonName");
        PokemonSprite = GetNodeOrNull<TextureRect>("PokemonSprite");

        _spriteCache = GetNodeOrNull<PokemonSpriteCache>("PokemonSpriteCache");
        _emotionHandler = GetNodeOrNull<EmotionHandler>("EmotionHandler");
        _rng = new Random();

        _spriteCache.TextureReady += _spriteCache_TextureReady;
        _spriteCache.TextureFailed += _spriteCache_TextureFailed;
    }

    private void _spriteCache_TextureFailed(string error)
    {
        GD.PrintErr(error);
    }

    private void _spriteCache_TextureReady(Texture2D texture)
    {
        PokemonSprite.Texture = texture;

        var contentSize = GetSize();
        var spriteSize = GetSpriteSize();

        //Position = new Vector2((contentSize.X - spriteSize.X) / 2, contentSize.Y - spriteSize.Y);

        //SpriteAutoCrop.ApplyCrop(PokemonSprite, 0.05f, true);
    }

    public void Init(PKM pokemon, PokemonWindow window)
    {
        _emotionHandler.Init(pokemon);
        _spriteCache.LoadOrDownloadTexture(pokemon, true);

        string name = pokemon.IsNicknamed ? pokemon.Nickname : GameInfo.Strings.Species.ElementAt(pokemon.Species);
        PokemonName.Text = name;

        _movementType = pokemon.GetMovementType();

        if (_movement != null)
        {
            _movement.Free();
        }

        var moveInfo = Movements.FirstOrDefault(x => x.Type == _movementType || true); //TODO TEST
        if (moveInfo != null)
        {
            //_movement = moveInfo.PackedScene.Instantiate<BaseMovement>();
            //AddChild(_movement);

            //_movement.Init(pokemon, window, this);
        }
    }

    public Vector2 GetSpriteSize()
    {
        return PokemonSprite.Texture.GetSize() * PokemonSprite.Scale;
    }
}
