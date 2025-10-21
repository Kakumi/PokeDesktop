using Godot;
using PKHeX.Core;
using PokeDesktop.utils;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PokemonFrame : Node2D
{
    [Export] public PokemonMovement[] Movements;
    [Export] public MovementType MovementType;

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
        PokemonSprite.Texture = texture;

        var windowSize = DisplayServer.WindowGetSize();

        float x = (float)_rng.NextDouble() * (windowSize.X - GetSize().X);
        float y = windowSize.Y - GetSize().Y;

        Position = new Vector2(x, y);

        SpriteAutoCrop.FitBottomTo(PokemonSprite);
    }

    public void Init(PKM pokemon)
    {
        _spriteCache.LoadOrDownloadTexture(pokemon, true);

        string name = pokemon.IsNicknamed ? pokemon.Nickname : GameInfo.Strings.Species.ElementAt(pokemon.Species);
        if (IsAllUpper(name))
        {
            name = name.ToPascalCase();
        }

        var textureSize = PokemonSprite.Texture.GetSize() * PokemonSprite.Scale;
        var textSize = SpeechBubble2D.GetRectSize();
        SpeechBubble2D.Offset = new Vector2((textureSize.X - textSize.X) / 2, -10);
        SpeechBubble2D.SetText(name);

        MovementType = GetMovementType(pokemon);

        if (_movement != null)
        {
            _movement.Free();
        }

        var moveInfo = Movements.FirstOrDefault(x => x.Type == MovementType);
        if (moveInfo != null)
        {
            _movement = moveInfo.PackedScene.Instantiate<BaseMovement>();
            AddChild(_movement);

            _movement.Init(pokemon, this);
        }
    }

    public Vector2 GetSize()
    {
        return PokemonSprite.Texture.GetSize() * PokemonSprite.Scale;
    }

    public Vector2[] GetRegions()
    {
        return
        [
            PokemonSprite.GlobalPosition,
            PokemonSprite.GlobalPosition + new Vector2(GetSize().X, 0),
            PokemonSprite.GlobalPosition + GetSize(),
            PokemonSprite.GlobalPosition + new Vector2(0, GetSize().Y)
        ];
    }

    private bool IsAllUpper(string input)
    {
        for (int i = 0; i < input.Length; i++)
        {
            if (!Char.IsUpper(input[i]))
            {
                return false;
            }
        }

        return true;
    }

    private bool HasType(PKM pkm, MoveType type)
    {
        return (MoveType)pkm.PersonalInfo.Type1 == type || (MoveType)pkm.PersonalInfo.Type2 == type;
    }

    private MovementType GetMovementType(PKM pokemon)
    {
        if (pokemon == null) return MovementType.None;

        Species sp = (Species)pokemon.Species;

        var TELEPORTERS = new HashSet<Species>
        {
            Species.Abra, Species.Kadabra, Species.Alakazam,
            Species.Ralts, Species.Kirlia, Species.Gardevoir, Species.Gallade,
            Species.Natu, Species.Xatu,
            Species.Elgyem, Species.Beheeyem,
            Species.Cosmog, Species.Cosmoem
        };

        var DIGGERS = new HashSet<Species>
        {
            Species.Diglett, Species.Dugtrio,
            Species.Drilbur, Species.Excadrill,
            Species.Sandshrew, Species.Sandslash,
            Species.Trapinch,
            Species.Onix, Species.Steelix,
            Species.Sandygast, Species.Palossand
        };

        var HOVERERS = new HashSet<Species>
        {
            Species.Magnemite, Species.Magneton, Species.Magnezone,
            Species.Rotom, // (les formes de Rotom sont identifiées par forme, ici on simplifie)
            Species.Koffing, Species.Weezing,
            Species.Gastly, Species.Haunter, Species.Gengar,
            Species.Misdreavus, Species.Mismagius,
            Species.Unown,
            Species.Eelektrik, Species.Eelektross,
            Species.Leavanny
    };

        var AQUATIC_FLYING_PREFER_SWIM = new HashSet<Species>
        {
            Species.Gyarados, Species.Mantine, Species.Pelipper, Species.Cramorant
        };

        if (TELEPORTERS.Contains(sp)) return MovementType.Teleport;
        if (DIGGERS.Contains(sp)) return MovementType.Dig;

        if (HasType(pokemon, MoveType.Flying) && !AQUATIC_FLYING_PREFER_SWIM.Contains(sp))
            return MovementType.Fly;

        if (HOVERERS.Contains(sp))
            return MovementType.Fly;

        if (HasType(pokemon, MoveType.Water))
            return MovementType.Swim;

        return MovementType.Walk;
    }
}
