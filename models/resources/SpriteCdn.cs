
using Godot;

public partial class SpriteCdn : Resource
{
    [Export] public bool Default { get; set; } = false;
    [Export] public string Name { get; set; }
    [Export] public string Folder { get; set; }
    [Export] public string NormalUrl { get; set; }
    [Export] public string ShinyUrl { get; set; }
}