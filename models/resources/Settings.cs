using Godot;
using PKHeX.Core;

public partial class Settings : Resource
{
    [Export] public string SaveFilePath { get; set; }
    [Export] public int ScreenIndex { get; set; } = DisplayServer.GetPrimaryScreen();
    [Export] public int MaxVisible { get; set; } = 6;
    [Export] public bool ShowName { get; set; } = true;
    [Export] public bool SmartMove { get; set; } = true;
    [Export] public bool DropMoney { get; set; } = true;
    [Export] public bool DropItem { get; set; } = true;
    [Export] public bool ShowEmotion { get; set; } = true;
    [Export] public bool AnimatedSprites { get; set; } = true;
    [Export] public bool CriesOnEmotion { get; set; } = true;
    [Export] public bool CriesOnClick { get; set; } = true;
    [Export] public int MinEmotionSeconds { get; set; } = 180;
    [Export] public int MaxEmotionSeconds { get; set; } = 300;
    [Export] public double PokemonScale { get; set; } = 1;
    [Export] public int CriesVolume { get; set; } = 60;
    [Export] public string Language { get; set; } = GameLanguage.DefaultLanguage;
}