using Godot;
using PKHeX.Core;
using System;
using System.Linq;

public partial class SettingsManager : Node
{
    public static SettingsManager Instance { get; private set; }
    public Settings Settings { get; private set; }

    private const string SETTINGS_PATH = "user://settings.tres";

    [Signal] public delegate void SettingsChangedEventHandler(Settings settings);

    public bool Loaded => Settings != null;

    public override void _Ready()
    {
        LoadSettings();
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
                UpdateSettings();

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

            UpdateSettings();

            EmitSignal(SignalName.SettingsChanged, Settings);
        }
        catch (Exception e)
        {
            GD.PrintErr($"[SettingsManager] Fail loading settings file: {e.Message}");
            SaveSettings();
        }
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

    private void UpdateSettings()
    {
        if (TranslationServer.GetLoadedLocales().Contains(Settings.Language))
        {
            TranslationServer.SetLocale(Settings.Language);
            GameInfo.Strings = GameInfo.GetStrings(Settings.Language);
        }
    }
}
