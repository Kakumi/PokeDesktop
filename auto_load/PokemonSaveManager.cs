using Godot;
using PKHeX.Core;
using System;
using System.Collections.Generic;

public partial class PokemonSaveManager : Node
{
    [Export] public PackedScene PokemonWindow;

    public static PokemonSaveManager Instance { get; private set; }

    private List<PokemonWindow> _popups;

    public override void _Ready()
    {
        _popups = new List<PokemonWindow>();

        SettingsManager.Instance.SettingsChanged += Instance_SettingsChanged;

        if (SettingsManager.Instance.Loaded)
        {
            Instance_SettingsChanged(SettingsManager.Instance.Settings);
        }
    }

    private void Instance_SettingsChanged(Settings settings)
    {
        LoadPokemons();
    }

    private void LoadPokemons()
    {
        var sav = SaveUtil.GetSaveFile(@"C:\Users\manga\Documents\PKHeX (25.08.30)a\TEST_SAVE.sav");
        if (sav == null)
        {
            throw new InvalidOperationException("TODO Invalid file.");
        }

        int max = 1;
        for (int i = 0; i < max && i < sav.PartyCount; i++)
        {
            var pkm = sav.PartyData[i];
            _popups.Add(CreatePokemonWindow(pkm));
        }
    }

    private PokemonWindow CreatePokemonWindow(PKM pokemon)
    {
        var window = PokemonWindow.Instantiate<PokemonWindow>();
        AddChild(window);

        window.Init(pokemon);

        return window;
    }

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
