using Godot;

public partial class Emotion : Resource
{
    [Export] public EmotionType Type { get; set; }
    [Export] public Texture2D Texture { get; set; }
}
