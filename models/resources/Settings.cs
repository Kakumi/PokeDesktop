using Godot;
using PKHeX.Core;

public partial class Settings : Resource
{
    [Export] public string SaveFilePath { get; set; }
    [Export] public int MaxVisible { get; set; } = 6;
    [Export] public bool ShowName { get; set; } = true;
    [Export] public bool SmartMove { get; set; } = true;
    [Export] public bool DropMoney { get; set; } = true;
    [Export] public bool DropItem { get; set; } = true;
    [Export] public bool ShowEmotion { get; set; } = true;
    [Export] public bool AnimatedSprites { get; set; } = true;
    [Export] public string Language { get; set; } = GameLanguage.DefaultLanguage;
}