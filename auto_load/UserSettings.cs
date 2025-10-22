using Godot;
using PKHeX.Core;
using System;
using System.Collections.Generic;

public partial class UserSettings : Node
{
    [Export] public PackedScene PokemonWindow;

    public static UserSettings Instance { get; private set; }
    public Settings Settings { get; private set; }
    public GameStrings GameStrings { get; private set; }

    private const string SETTINGS_PATH = "user://settings.tres";
    private List<PokemonWindow> _popups;

    [Signal] public delegate void SettingsChangedEventHandler(Settings settings);

    public override void _Ready()
    {
        _popups = new List<PokemonWindow>();

        LoadSettings();

        GameStrings = GameInfo.Strings;

        LoadPokemons();
    }

    public void SaveSettings()
    {
        try
        {
            if (Settings == null)
            {
                Settings = new Settings();
            }

            var result = ResourceSaver.Save(Settings, SETTINGS_PATH);
            if (result == Error.Ok)
            {
                EmitSignal(SignalName.SettingsChanged, Settings);
                GD.Print($"[SettingsManager] Settings saved at {SETTINGS_PATH}");
            }
            else
            {
                GD.PrintErr($"[SettingsManager] Can't save settings at {SETTINGS_PATH}");
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"[SettingsManager] Fail saving settings: {e.Message}");
        }
    }

    public void LoadSettings()
    {
        if (!FileAccess.FileExists(SETTINGS_PATH))
        {
            GD.Print($"[SettingsManager] Fichier {SETTINGS_PATH} introuvable, création avec valeurs par défaut.");
            SaveSettings();
            return;
        }

        try
        {
            Settings = ResourceLoader.Load<Settings>(SETTINGS_PATH) ?? new Settings();

            EmitSignal(SignalName.SettingsChanged, Settings);
        }
        catch (Exception e)
        {
            GD.PrintErr($"[SettingsManager] Fail loading settings file: {e.Message}");
            SaveSettings();
        }
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
            var pkm = sav.PartyData[3]; //TODO TEST
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
