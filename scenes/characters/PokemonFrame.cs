using Godot;
using PKHeX.Core;
using PokeDesktop.utils;
using System;
using System.Linq;

public partial class PokemonFrame : Node2D
{
    [Export] public PokemonMovement[] Movements;
    [Export] public MovementType MovementType;
    [Export] public int LabelOffset = 10;

    public Sprite2D PokemonSprite { get; private set; }
    public SpeechBubble2D SpeechBubble2D { get; private set; }

    private PokemonSpriteCache _spriteCache;
    private Random _rng;
    private BaseMovement _movement;

    public override void _Ready()
    {
        PokemonSprite = GetNodeOrNull<Sprite2D>("PokemonSprite");
        SpeechBubble2D = GetNodeOrNull<SpeechBubble2D>("SpeechBubble2D");

        _spriteCache = GetNodeOrNull<PokemonSpriteCache>("PokemonSpriteCache");
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
        var t = GetParent().GetNode<TextureRect>("View/TextureRect");
        t.Texture = texture;
        PokemonSprite.Texture = texture;

        var contentSize = GetSize();
        var spriteSize = GetSpriteSize();
        var textSize = SpeechBubble2D.GetLabelRect().Size;

        Position = new Vector2((contentSize.X - spriteSize.X) / 2, contentSize.Y - spriteSize.Y);
        SpeechBubble2D.Offset = new Vector2((spriteSize.X - textSize.X) / 2, -LabelOffset);
        SpeechBubble2D.QueueRedraw();

        SpriteAutoCrop.FitBottomTo(PokemonSprite);
        SpriteAutoCrop.ApplyCrop(t, 0.05f, true);
    }

    public void Init(PKM pokemon, PokemonWindow window)
    {
        _spriteCache.LoadOrDownloadTexture(pokemon, true);

        string name = pokemon.IsNicknamed ? pokemon.Nickname : GameInfo.Strings.Species.ElementAt(pokemon.Species);

        var contentSize = GetSize();
        var spriteSize = GetSpriteSize();
        var textSize = SpeechBubble2D.GetLabelRect().Size;
        SpeechBubble2D.Offset = new Vector2((spriteSize.X - textSize.X) / 2, -LabelOffset);
        SpeechBubble2D.SetText(name);

        MovementType = pokemon.GetMovementType();

        if (_movement != null)
        {
            _movement.Free();
        }

        var moveInfo = Movements.FirstOrDefault(x => x.Type == MovementType);
        if (moveInfo != null)
        {
            _movement = moveInfo.PackedScene.Instantiate<BaseMovement>();
            AddChild(_movement);

            //_movement.Init(pokemon, window, this);
        }
    }

    public Vector2 GetSpriteSize()
    {
        return PokemonSprite.Texture.GetSize() * PokemonSprite.Scale;
    }

    public Vector2 GetSize()
    {
        var spriteSize = GetSpriteSize();
        var textSize = SpeechBubble2D.GetLabelRect().Size;

        return new Vector2(Math.Max(spriteSize.X, textSize.X), spriteSize.Y + textSize.Y + LabelOffset);
    }
}
