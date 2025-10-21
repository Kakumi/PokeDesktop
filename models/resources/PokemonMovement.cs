using Godot;

public partial class PokemonMovement : Resource
{
    [Export] public MovementType Type { get; set; }
    [Export] public PackedScene PackedScene { get; set; }
}
