using Godot;
using PKHeX.Core;
using System;
using System.Linq;

public partial class SettingsPanel : VBoxContainer
{
    public Label PathLabel { get; private set; }
    public Label ErrorLabel { get; private set; }
    public Label SuccessLabel { get; private set; }

    public OptionButton LanguagesBox { get; private set; }
    public TextureButton OpenSaveFileButton { get; private set; }
    public FileDialog FileDialog { get; private set; }
    public CheckButton ShowName { get; private set; }
    public CheckButton SmartMove { get; private set; }
    public CheckButton DropMoney { get; private set; }
    public CheckButton DropItem { get; private set; }
    public CheckButton ShowEmotion { get; private set; }
    public CheckButton AnimatedSprites { get; private set; }
    public SpinBox MaxVisible { get; private set; }
    public Button SaveButton { get; private set; }

    public override void _Ready()
    {
        var settingsContainer = GetNode("SettingsContainer");
        PathLabel = settingsContainer.GetNode<Label>("SavePath/HBoxContainer/Path");
        ErrorLabel = settingsContainer.GetNode<Label>("SavePath/HBoxContainer/ErrorLabel");
        SuccessLabel = settingsContainer.GetNode<Label>("SavePath/HBoxContainer/SuccessLabel");

        LanguagesBox = settingsContainer.GetNode<OptionButton>("Language/LanguagesBox");
        FileDialog = settingsContainer.GetNode<FileDialog>("SavePath/FileDialog");
        OpenSaveFileButton = settingsContainer.GetNode<TextureButton>("SavePath/MarginContainer/OpenSaveFileButton");
        ShowName = settingsContainer.GetNode<CheckButton>("ShowName");
        SmartMove = settingsContainer.GetNode<CheckButton>("SmartMove");
        DropMoney = settingsContainer.GetNode<CheckButton>("DropMoney");
        DropItem = settingsContainer.GetNode<CheckButton>("DropItem");
        ShowEmotion = settingsContainer.GetNode<CheckButton>("ShowEmotion");
        AnimatedSprites = settingsContainer.GetNode<CheckButton>("AnimatedSprites");
        MaxVisible = settingsContainer.GetNode<SpinBox>("HBoxContainer/MaxVisible");
        SaveButton = GetNode<Button>("SaveButton");

        OpenSaveFileButton.Pressed += OpenSaveFileButton_Pressed;
        LanguagesBox.ItemSelected += LanguagesBox_ItemSelected;
        MaxVisible.ValueChanged += MaxVisible_ValueChanged;
        SaveButton.Pressed += SaveButton_Pressed;
        ShowName.Pressed += ShowName_Pressed;
        SmartMove.Pressed += SmartMove_Pressed;
        DropMoney.Pressed += DropMoney_Pressed;
        DropItem.Pressed += DropItem_Pressed;
        ShowEmotion.Pressed += ShowEmotion_Pressed;
        AnimatedSprites.Pressed += AnimatedSprites_Pressed;
        FileDialog.FileSelected += FileDialog_FileSelected;

        ErrorLabel.Visible = false;
        SuccessLabel.Visible = false;

        Init();
    }

    private void Init()
    {
        var settings = SettingsManager.Instance.Settings;

        PathLabel.Text = settings.SaveFilePath;
        ShowName.ButtonPressed = settings.ShowName;
        SmartMove.ButtonPressed = settings.SmartMove;
        DropMoney.ButtonPressed = settings.DropMoney;
        DropItem.ButtonPressed = settings.DropItem;
        ShowEmotion.ButtonPressed = settings.ShowEmotion;
        AnimatedSprites.ButtonPressed = settings.AnimatedSprites;
        MaxVisible.Value = settings.MaxVisible;

        if (settings.SaveFilePath != null)
        {
            TrySaveFile(settings.SaveFilePath);
        }

        InitLanguages(settings);
    }

    private void InitLanguages(Settings settings)
    {
        var appLanguages = TranslationServer.GetLoadedLocales();
        var supportedLanguages = GameLanguage.AllSupportedLanguages.ToArray()
            .Where(x => appLanguages.Contains(x));

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
            SettingsManager.Instance.Settings.SaveFilePath = path;
        }
    }

    private bool TrySaveFile(string path)
    {
        if (SaveUtil.TryGetSaveFile(path, out var save))
        {
            ErrorLabel.Visible = false;
            SuccessLabel.Text = string.Format(TranslationServer.Translate("SETTINGS_WELCOME_SAVE_FILE"), save.OT);
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
    }

    private void SaveButton_Pressed()
    {
        SettingsManager.Instance.SaveSettings();

        //Update custom texts
        TrySaveFile(SettingsManager.Instance.Settings.SaveFilePath);
    }
}
