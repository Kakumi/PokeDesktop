using Godot;
using PKHeX.Core;
using System;

public partial class BaseMovement : Node2D
{
    [Export] public float BaseSpeedPxPerSec = 80f;
    [Export] public float IvSpeedFactor = 3.0f;
    [Export] public float IdleMin = 1.0f;
    [Export] public float IdleMax = 4.0f;
    [Export] public float WalkDistMin = 60f;
    [Export] public float WalkDistMax = 300f;
    [Export] public float WalkTriggerChance = 0.6f;
    [Export] public float BobAmplitude = 0f; //7
    [Export] public float BobFrequency = 0f; //2
    [Export] public int OffsetY { get; set; } = 0;

    public PKM Pokemon { get; private set; }
    public PokemonWindow WindowToMove { get; private set; }
    public PokemonInstance Instance { get; private set; }

    protected enum State { Idle, Walking }
    protected State _state = State.Idle;
    protected float _stateTimer = 0f;

    protected Vector2I _startPos;
    protected Vector2I _targetPos;
    protected float _walkDuration = 1f;
    protected float _walkElapsed = 0f;
    protected float _dirSign = 1f;

    protected readonly Random _rng = new();

    public void Init(PKM pkm, PokemonWindow window, PokemonInstance instance)
    {
        Pokemon = pkm;
        WindowToMove = window;
        Instance = instance;

        EnterIdle();
    }

    public int GetWinBaseY()
    {
        return WindowToMove.GetWindowDefaultPosition().Y - OffsetY;
    }

    public override void _Process(double delta)
    {
        switch (_state)
        {
            case State.Idle:
                UpdateIdle((float)delta);
                break;

            case State.Walking:
                UpdateCustomAnimation((float)delta);
                break;
        }
    }

    protected virtual void EnterCustomAnimation()
    {

    }

    protected virtual void UpdateCustomAnimation(float delta)
    {

    }

    // ----------- IDLE -----------

    protected void EnterIdle()
    {
        _state = State.Idle;
        _stateTimer = RandRange(IdleMin, IdleMax);
        WindowToMove.Position = new Vector2I(WindowToMove.Position.X, GetWinBaseY());
    }

    private void UpdateIdle(float dt)
    {
        _stateTimer -= dt;
        if (_stateTimer <= 0f)
        {
            if (_rng.NextDouble() < WalkTriggerChance)
                EnterCustomAnimation();
            else
                EnterIdle();
        }
    }

    // ----------- Utils -----------

    protected float RandRange(float a, float b) => (float)(_rng.NextDouble() * (b - a) + a);

    protected float EaseInOut(float t)
    {
        // S-curve
        return t * t * (3f - 2f * t);
    }

    protected int GetSpeedIV(PKM p)
    {
        var speedIv = p.IV_SPE;
        return Mathf.Clamp(speedIv, 0, 31);
    }
}
