using Godot;
using System;

public partial class Walker : BaseMovement
{
    protected override void EnterCustomAnimation()
    {
        _state = State.Walking;
        _walkElapsed = 0f;

        // Destination horizontale aléatoire (à l’écran / fenêtre)
        float dist = RandRange(WalkDistMin, WalkDistMax);
        _dirSign = _rng.Next(0, 2) == 0 ? -1f : 1f;

        // Vitesse = base + IV*factor
        int iv = GetSpeedIV(Pokemon);
        float speed = Math.Max(5f, BaseSpeedPxPerSec + IvSpeedFactor * iv); // clamp minimal
        _walkDuration = dist / speed;

        _startPos = WindowToMove.Position;

        //For Window
        var usable = WindowToMove.GetWindowUsable();
        float targetX = Mathf.Clamp(_startPos.X + _dirSign * dist, usable.X, usable.Y);
        _targetPos = new Vector2I((int)targetX, GetWinBaseY());

        if (Instance.PokemonSprite != null) Instance.PokemonSprite.FlipH = _dirSign > 0f;
    }

    protected override void UpdateCustomAnimation(float dt)
    {
        _walkElapsed += dt;
        float t = Mathf.Clamp(_walkElapsed / _walkDuration, 0f, 1f);
        float eased = EaseInOut(t);
        float bob = (BobAmplitude > 0f && BobFrequency > 0f)
            ? Mathf.Sin(_walkElapsed * MathF.Tau * BobFrequency) * BobAmplitude
            : 0f;

        Vector2 ceilPos = new Vector2(_startPos.X, _startPos.Y);
        Vector2 lerp = ceilPos.Lerp(_targetPos, eased);

        var newPos = new Vector2I((int)MathF.Round(lerp.X), GetWinBaseY() + (int)MathF.Round(bob));
        WindowToMove.Position = newPos;

        if (t >= 1f)
        {
            EnterIdle();
        }
    }
}
