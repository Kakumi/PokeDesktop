using Godot;

public partial class CriesCdn : Resource
{
    [Export] public bool Default { get; set; } = false;
    [Export] public string Name { get; set; }
    [Export] public string Folder { get; set; }
    [Export] public string Url { get; set; }
}