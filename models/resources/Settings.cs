using Godot;
using PKHeX.Core;

public partial class Settings : Resource
{
    public string SaveFilePath { get; set; }
    public int MaxVisible { get; set; } = 6;
    public bool ShowName { get; set; } = true;
    public bool SmartMove { get; set; } = true;
    public bool DropMoney { get; set; } = true;
    public bool DropItem { get; set; } = true;
    public bool ShowEmotion { get; set; } = true;
    public string Language { get; set; } = GameLanguage.DefaultLanguage;
}