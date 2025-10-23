using Godot;
using System;

public partial class RandomTimer : Timer
{
    [Export] public int MinTimerSeconds { get; set; } = 180;
    [Export] public int MaxTimerSeconds { get; set; } = 300;

    private Random _rng = new Random();

    public override void _Ready()
    {
        OneShot = true;
    }

    public void StartNewTimer()
    {
        Start(_rng.NextDouble() * (MaxTimerSeconds - MinTimerSeconds) + MinTimerSeconds);
    }
}
