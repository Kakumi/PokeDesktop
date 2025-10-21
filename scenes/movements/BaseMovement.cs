using Godot;
using PKHeX.Core;
using System;

public partial class BaseMovement : Node2D
{
    [Export] public float BaseSpeedPxPerSec = 80f;   // vitesse de base (px/s)
    [Export] public float IvSpeedFactor = 3.0f;      // combien 1 point d’IV ajoute en px/s
    [Export] public float IdleMin = 1.0f;            // temps idle min (s)
    [Export] public float IdleMax = 4.0f;            // temps idle max (s)
    [Export] public float WalkDistMin = 60f;         // distance de marche min (px)
    [Export] public float WalkDistMax = 300f;        // distance de marche max (px)
    [Export] public float WalkTriggerChance = 0.35f; // proba de déclencher une marche quand l’idle se termine
    [Export] public float BobAmplitude = 0f;         // amplitude du rebond (px)
    [Export] public float BobFrequency = 0f;        // fréquence du rebond (Hz)

    public PKM Pokemon { get; private set; }
    public PokemonFrame PokemonFrame { get; private set; }

    protected enum State { Idle, Walking }
    protected State _state = State.Idle;
    protected float _stateTimer = 0f;

    protected Vector2 _startPos;
    protected Vector2 _targetPos;
    protected float _walkDuration = 1f;
    protected float _walkElapsed = 0f;
    protected float _dirSign = 1f;
    protected float _spriteBaseY;

    protected readonly Random _rng = new();

    public void Init(PKM pkm, PokemonFrame frame)
    {
        Pokemon = pkm;
        PokemonFrame = frame;
        _spriteBaseY = Position.Y;

        EnterIdle();
    }

    public override void _Ready()
    {
        if (Pokemon == null)
        {
            _spriteBaseY = Position.Y;
        }
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
        Position = new Vector2(Position.X, _spriteBaseY);
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
        // S-curve simple
        return t * t * (3f - 2f * t);
    }

    protected int GetSpeedIV(PKM p)
    {
        var speedIv = p.IV_SPE;
        return Mathf.Clamp(speedIv, 0, 31);
    }
}
