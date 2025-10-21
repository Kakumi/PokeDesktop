using Godot;
using PKHeX.Core;
using System;
using System.Collections.Generic;

public partial class UserSettings : Node
{
    [Export] public PackedScene PokemonWindow;

    public Settings Settings { get; private set; }

    private const string SETTINGS_PATH = "user://settings.json";
    private List<PokemonPopup> _popups;

    [Signal] public delegate void SettingsChangedEventHandler(Settings settings);

    public override void _Ready()
    {
        _popups = new List<PokemonPopup>();

        LoadSettings();

        LoadPokemons();
    }

    public void SaveSettings()
    {
        try
        {
            ResourceSaver.Save(Settings, SETTINGS_PATH);
            //string jsonString = JsonSerializer.Serialize(Settings);

            //using var file = FileAccess.Open(SETTINGS_PATH, FileAccess.ModeFlags.Write);
            //file.StoreString(jsonString);

            EmitSignal(SignalName.SettingsChanged, Settings);

            GD.Print($"[SettingsManager] Settings saved at {SETTINGS_PATH}");
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
            //using var file = FileAccess.Open(SETTINGS_PATH, FileAccess.ModeFlags.Read);
            //string jsonString = file.GetAsText();

            //JsonSerializerOptions jsonOpts = new()
            //{
            //    WriteIndented = true,
            //};

            //Settings = JsonSerializer.Deserialize<Settings>(jsonString, jsonOpts);
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
            var pkm = sav.PartyData[i];
            _popups.Add(CreatePokemonWindow(pkm));
        }
    }

    private PokemonPopup CreatePokemonWindow(PKM pokemon)
    {
        var window = PokemonWindow.Instantiate<PokemonPopup>();
        AddChild(window);

        window.Init(pokemon);

        return window;
    }
}
