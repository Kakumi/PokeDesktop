using Godot;
using PKHeX.Core;
using System.Collections.Generic;
using System.Linq;

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
        foreach (var item in GetChildren().Where(x => x is PokemonWindow))
        {
            item.QueueFree();
        }

        LoadPokemons();
    }

    private void LoadPokemons()
    {
        string path = SettingsManager.Instance.Settings.SaveFilePath;
        if (path == null)
        {
            return;
        }

        var sav = SaveUtil.GetSaveFile(path);
        if (sav == null)
        {
            return;
        }

        int max = SettingsManager.Instance.Settings.MaxVisible;
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
