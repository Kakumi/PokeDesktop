using Godot;
using PKHeX.Core;
using System.Linq;

public partial class SettingsPanel : VBoxContainer
{
    public Label PathLabel { get; private set; }

    public OptionButton LanguagesBox { get; private set; }
    public TextureButton OpenSaveFileButton { get; private set; }
    public FileDialog FileDialog { get; private set; }
    public CheckButton ShowName { get; private set; }
    public CheckButton SmartMove { get; private set; }
    public CheckButton DropMoney { get; private set; }
    public CheckButton DropItem { get; private set; }
    public CheckButton ShowEmotion { get; private set; }
    public Button SaveButton { get; private set; }

    public override void _Ready()
    {
        var settingsContainer = GetNode("SettingsContainer");
        PathLabel = settingsContainer.GetNode<Label>("SavePath/HBoxContainer/Path");

        LanguagesBox = settingsContainer.GetNode<OptionButton>("Language/LanguagesBox");
        FileDialog = settingsContainer.GetNode<FileDialog>("SavePath/FileDialog");
        OpenSaveFileButton = settingsContainer.GetNode<TextureButton>("SavePath/TextureButton");
        ShowName = settingsContainer.GetNode<CheckButton>("ShowName");
        SmartMove = settingsContainer.GetNode<CheckButton>("SmartMove");
        DropMoney = settingsContainer.GetNode<CheckButton>("DropMoney");
        DropItem = settingsContainer.GetNode<CheckButton>("DropItem");
        ShowEmotion = settingsContainer.GetNode<CheckButton>("ShowEmotion");
        SaveButton = GetNode<Button>("SaveButton");

        OpenSaveFileButton.Pressed += OpenSaveFileButton_Pressed;
        LanguagesBox.ItemSelected += LanguagesBox_ItemSelected;
        SaveButton.Pressed += SaveButton_Pressed;

        Init();
    }

    private void SaveButton_Pressed()
    {
        SettingsManager.Instance.SaveSettings();
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

    private void OpenSaveFileButton_Pressed()
    {
        FileDialog.Show();
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
            LanguagesBox.Select((int)GameLanguage.GetLanguage(settings.Language));
        }
    }
}
