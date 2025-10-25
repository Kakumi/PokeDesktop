using Godot;
using PKHeX.Core;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class SettingsPanel : VBoxContainer
{
    public Label PathLabel { get; private set; }
    public Label ErrorLabel { get; private set; }
    public Label SuccessLabel { get; private set; }
    public Label CriesVolumeSliderLabel { get; private set; }

    public OptionButton ScreensBox { get; private set; }
    public OptionButton LanguagesBox { get; private set; }
    public TextureButton OpenSaveFileButton { get; private set; }
    public FileDialog FileDialog { get; private set; }
    public CheckButton ShowName { get; private set; }
    public CheckButton SmartMove { get; private set; }
    public CheckButton DropMoney { get; private set; }
    public CheckButton DropItem { get; private set; }
    public CheckButton ShowEmotion { get; private set; }
    public CheckButton AnimatedSprites { get; private set; }
    public CheckButton CriesOnEmotion { get; private set; }
    public CheckButton CriesOnClick { get; private set; }
    public SpinBox MinEmotionSeconds { get; private set; }
    public SpinBox MaxEmotionSeconds { get; private set; }
    public SpinBox MaxVisible { get; private set; }
    public SpinBox PokemonScale { get; private set; }
    public Slider CriesVolumeSlider { get; private set; }
    public Button SaveButton { get; private set; }

    public override void _Ready()
    {
        var settingsContainer = GetNode("ScrollContainer/SettingsContainer");
        PathLabel = settingsContainer.GetNode<Label>("SavePath/HBoxContainer/Path");
        ErrorLabel = settingsContainer.GetNode<Label>("SavePath/HBoxContainer/ErrorLabel");
        SuccessLabel = settingsContainer.GetNode<Label>("SavePath/HBoxContainer/SuccessLabel");
        CriesVolumeSliderLabel = settingsContainer.GetNode<Label>("CriesVolume/CriesVolumeSliderLabel");

        ScreensBox = settingsContainer.GetNode<OptionButton>("Screen/ScreensBox");
        LanguagesBox = settingsContainer.GetNode<OptionButton>("Language/LanguagesBox");
        FileDialog = settingsContainer.GetNode<FileDialog>("SavePath/FileDialog");
        OpenSaveFileButton = settingsContainer.GetNode<TextureButton>("SavePath/MarginContainer/OpenSaveFileButton");
        ShowName = settingsContainer.GetNode<CheckButton>("ShowName");
        SmartMove = settingsContainer.GetNode<CheckButton>("SmartMove");
        DropMoney = settingsContainer.GetNode<CheckButton>("DropMoney");
        DropItem = settingsContainer.GetNode<CheckButton>("DropItem");
        ShowEmotion = settingsContainer.GetNode<CheckButton>("ShowEmotion");
        AnimatedSprites = settingsContainer.GetNode<CheckButton>("AnimatedSprites");
        CriesOnEmotion = settingsContainer.GetNode<CheckButton>("CriesOnEmotion");
        CriesOnClick = settingsContainer.GetNode<CheckButton>("CriesOnClick");
        MinEmotionSeconds = settingsContainer.GetNode<SpinBox>("EmotionTimer/MinEmotionSeconds");
        MaxEmotionSeconds = settingsContainer.GetNode<SpinBox>("EmotionTimer/MaxEmotionSeconds");
        MaxVisible = settingsContainer.GetNode<SpinBox>("PokemonAmount/MaxVisible");
        PokemonScale = settingsContainer.GetNode<SpinBox>("PokemonScale/PokemonScale");
        CriesVolumeSlider = settingsContainer.GetNode<Slider>("CriesVolume/CriesVolumeSlider");
        SaveButton = GetNode<Button>("SaveButton");

        OpenSaveFileButton.Pressed += OpenSaveFileButton_Pressed;
        ScreensBox.ItemSelected += ScreensBox_ItemSelected;
        LanguagesBox.ItemSelected += LanguagesBox_ItemSelected;
        MaxVisible.ValueChanged += MaxVisible_ValueChanged;
        PokemonScale.ValueChanged += PokemonScale_ValueChanged;
        SaveButton.Pressed += SaveButton_Pressed;
        ShowName.Pressed += ShowName_Pressed;
        SmartMove.Pressed += SmartMove_Pressed;
        DropMoney.Pressed += DropMoney_Pressed;
        DropItem.Pressed += DropItem_Pressed;
        ShowEmotion.Pressed += ShowEmotion_Pressed;
        CriesOnEmotion.Pressed += UseRandomCries_Pressed;
        CriesOnClick.Pressed += CriesOnClick_Pressed;
        AnimatedSprites.Pressed += AnimatedSprites_Pressed;
        FileDialog.FileSelected += FileDialog_FileSelected;
        CriesVolumeSlider.ValueChanged += CriesVolumeSlider_ValueChanged;

        ErrorLabel.Visible = false;
        SuccessLabel.Visible = false;

        Init();
    }

    private void Init()
    {
        var settings = SettingsManager.Instance.Settings;

        PathLabel.Text = settings.SaveFilePath;
        CriesVolumeSliderLabel.Text = string.Format(TranslationServer.Translate("SETTINGS_CRIES_VOLUME_PRC"), settings.CriesVolume);

        ShowName.ButtonPressed = settings.ShowName;
        SmartMove.ButtonPressed = settings.SmartMove;
        DropMoney.ButtonPressed = settings.DropMoney;
        DropItem.ButtonPressed = settings.DropItem;
        ShowEmotion.ButtonPressed = settings.ShowEmotion;
        AnimatedSprites.ButtonPressed = settings.AnimatedSprites;
        CriesOnEmotion.ButtonPressed = settings.CriesOnEmotion;
        CriesOnClick.ButtonPressed = settings.CriesOnClick;
        MaxVisible.Value = settings.MaxVisible;
        PokemonScale.Value = settings.PokemonScale;
        CriesVolumeSlider.Value = settings.CriesVolume;
        MinEmotionSeconds.Value = settings.MinEmotionSeconds;
        MaxEmotionSeconds.Value = settings.MaxEmotionSeconds;

        MaxEmotionSeconds.MinValue = settings.MinEmotionSeconds;
        MinEmotionSeconds.MaxValue = settings.MaxEmotionSeconds;

        if (settings.SaveFilePath != null)
        {
            TrySaveFile(settings.SaveFilePath);
        }

        InitLanguages(settings);
        InitScreens(settings);
    }

    private void InitLanguages(Settings settings)
    {
        var appLanguages = TranslationServer.GetLoadedLocales();
        var supportedLanguages = new List<string>();
        for (int i = 0; i < GameLanguage.LanguageCount; i++)
        {
            var lang = GameLanguage.LanguageCode(i);
            if (appLanguages.Contains(lang))
            {
                supportedLanguages.Add(lang);
            }
        }
        //NET 9
        //var supportedLanguages = GameLanguage.AllSupportedLanguages.ToArray()
        //    .Where(x => appLanguages.Contains(x));

        var languages = supportedLanguages
            .Select(x => new { Id = GameLanguage.GetLanguageIndex(x), Locale = TranslationServer.Translate("LANG_" + x) });

        foreach (var language in languages)
        {
            LanguagesBox.AddItem(language.Locale, language.Id);
        }

        if (supportedLanguages.Contains(settings.Language))
        {
            var languageIndex = Array.IndexOf(supportedLanguages.ToArray(), settings.Language);
            LanguagesBox.Select(languageIndex);
        }
    }

    private void InitScreens(Settings settings)
    {
        int screenCount = DisplayServer.GetScreenCount();
        var dictionary = new Dictionary<int, string>();
        for (int i = 0; i < screenCount; i++)
        {
            ScreensBox.AddItem(string.Format(TranslationServer.Translate("MONITOR"), i), i);
            if (settings.ScreenIndex == i)
            {
                ScreensBox.Select(i);
            }
        }
    }

    private void AnimatedSprites_Pressed()
    {
        SettingsManager.Instance.Settings.AnimatedSprites = AnimatedSprites.ButtonPressed;
    }

    private void ShowEmotion_Pressed()
    {
        SettingsManager.Instance.Settings.ShowEmotion = ShowEmotion.ButtonPressed;
    }

    private void DropItem_Pressed()
    {
        SettingsManager.Instance.Settings.DropItem = DropItem.ButtonPressed;
    }

    private void DropMoney_Pressed()
    {
        SettingsManager.Instance.Settings.DropMoney = DropMoney.ButtonPressed;
    }

    private void SmartMove_Pressed()
    {
        SettingsManager.Instance.Settings.SmartMove = SmartMove.ButtonPressed;
    }

    private void ShowName_Pressed()
    {
        SettingsManager.Instance.Settings.ShowName = ShowName.ButtonPressed;
    }

    private void UseRandomCries_Pressed()
    {
        SettingsManager.Instance.Settings.CriesOnEmotion = CriesOnEmotion.ButtonPressed;
    }

    private void CriesOnClick_Pressed()
    {
        SettingsManager.Instance.Settings.CriesOnClick = CriesOnClick.ButtonPressed;
    }

    private void CriesVolumeSlider_ValueChanged(double value)
    {
        SettingsManager.Instance.Settings.CriesVolume = (int)Math.Clamp(value, 0, 100);
        CriesVolumeSliderLabel.Text = string.Format(TranslationServer.Translate("SETTINGS_CRIES_VOLUME_PRC"), value);
    }

    private void PokemonScale_ValueChanged(double value)
    {
        SettingsManager.Instance.Settings.PokemonScale = value;
    }

    private void ScreensBox_ItemSelected(long index)
    {
        var screenId = ScreensBox.GetItemId((int)index);
        if (screenId < DisplayServer.GetScreenCount())
        {
            SettingsManager.Instance.Settings.ScreenIndex = screenId;
        }
    }

    private void LanguagesBox_ItemSelected(long index)
    {
        var languageId = LanguagesBox.GetItemId((int)index);
        var newLang = GameLanguage.LanguageCode(languageId);
        if (TranslationServer.GetLoadedLocales().Contains(newLang))
        {
            SettingsManager.Instance.Settings.Language = newLang;
        }
    }

    private void MaxVisible_ValueChanged(double value)
    {
        SettingsManager.Instance.Settings.MaxVisible = (int)value;
    }

    private void FileDialog_FileSelected(string path)
    {
        if (TrySaveFile(path))
        {
            PathLabel.Text = path;
            SettingsManager.Instance.Settings.SaveFilePath = path;
        }
    }

    private bool TrySaveFile(string path)
    {
        var save = SaveUtil.GetVariantSAV(path);
        //NET 9
        // if (SaveUtil.TryGetSaveFile(path, out var save))
        if (save != null)
        {
            ErrorLabel.Visible = false;
            SuccessLabel.Text = string.Format(TranslationServer.Translate("WELCOME_TRAINER"), save.OT);
            SuccessLabel.Visible = true;
            return true;
        }

        ErrorLabel.Visible = true;
        SuccessLabel.Visible = false;
        return false;
    }

    private void OpenSaveFileButton_Pressed()
    {
        FileDialog.Show();
        FileDialog.CurrentScreen = GetViewport().GetWindow().CurrentScreen;
        FileDialog.MoveToCenter();
    }

    private void SaveButton_Pressed()
    {
        SettingsManager.Instance.SaveSettings();

        //Update custom texts
        TrySaveFile(SettingsManager.Instance.Settings.SaveFilePath);
    }
}
