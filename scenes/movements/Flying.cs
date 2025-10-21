using Godot;
using System;

public partial class Flying : BaseMovement
{
    protected override void EnterCustomAnimation()
    {
        _state = State.Walking;
        _walkElapsed = 0f;

        float dist = RandRange(WalkDistMin, WalkDistMax);
        _dirSign = _rng.Next(0, 2) == 0 ? -1f : 1f;

        // Vitesse = base + IV*factor
        int iv = GetSpeedIV(Pokemon);
        float speed = Math.Max(5f, BaseSpeedPxPerSec + IvSpeedFactor * iv); // clamp minimal
        _walkDuration = dist / speed;

        _startPos = PokemonFrame.Position;

        var viewW = GetViewport().GetVisibleRect().Size.X;
        float targetX = Mathf.Clamp(_startPos.X + _dirSign * dist, 0f, viewW);
        _targetPos = new Vector2(targetX, _spriteBaseY);

        if (PokemonFrame.PokemonSprite != null) PokemonFrame.PokemonSprite.FlipH = _dirSign > 0f;
    }

    protected override void UpdateCustomAnimation(float dt)
    {
        _walkElapsed += dt;
        float t = Mathf.Clamp(_walkElapsed / _walkDuration, 0f, 1f);

        float eased = EaseInOut(t);
        Vector2 pos = _startPos.Lerp(_targetPos, eased);

        pos.Y = _spriteBaseY + Mathf.Sin(_walkElapsed * MathF.Tau * BobFrequency) * BobAmplitude;

        PokemonFrame.Position = pos;

        if (t >= 1f)
            EnterIdle();
    }
}
