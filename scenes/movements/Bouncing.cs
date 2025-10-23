using Godot;
using System;

public partial class Bouncing : BaseMovement
{
    // Tracks Y-base and compensates for layout jumps when the window resizes
    private float _baseYPrev;
    private float _offsetYComp;   // temporary compensation offset (decays toward 0)

    protected override void EnterCustomAnimation()
    {
        _state = State.Walking;
        _walkElapsed = 0f;

        float dist = RandRange(WalkDistMin, WalkDistMax);
        _dirSign = _rng.Next(0, 2) == 0 ? -1f : 1f;

        int iv = GetSpeedIV(Pokemon);
        float speed = Math.Max(5f, BaseSpeedPxPerSec + IvSpeedFactor * iv);
        _walkDuration = dist / speed;

        _startPos = WindowToMove.Position;

        // Only move horizontally — the Y base is read dynamically every frame
        var usable = WindowToMove.GetWindowUsable();
        float targetX = Mathf.Clamp(_startPos.X + _dirSign * dist, usable.X, usable.Y);
        _targetPos = new Vector2I((int)targetX, _startPos.Y);

        // Initialize the base and compensation
        _baseYPrev = GetWinBaseY();
        _offsetYComp = 0f;

        if (Instance.PokemonSprite != null)
            Instance.PokemonSprite.FlipH = _dirSign > 0f;
    }

    protected override void UpdateCustomAnimation(float dt)
    {
        _walkElapsed += dt;

        float t = Mathf.Clamp(_walkElapsed / _walkDuration, 0f, 1f);
        float eased = EaseInOut(t);

        // --- Dynamic base Y + anti-pop compensation ---
        float baseYNow = GetWinBaseY();
        float deltaBase = baseYNow - _baseYPrev;

        if (Mathf.Abs(deltaBase) > 0.5f)
        {
            // Instantly cancel the visual jump caused by layout change
            _offsetYComp -= deltaBase;
        }
        _baseYPrev = baseYNow;

        // Gradually reduce the compensation back to zero (smooth catch-up)
        // k ≈ 12 → ~120 ms half-life (adjust if needed)
        float k = 12f;
        float alpha = 1f - Mathf.Exp(-k * dt);
        _offsetYComp = Mathf.Lerp(_offsetYComp, 0f, alpha);

        // --- Walking bob: goes up then down ---
        // In Godot 2D: Y increases downward → use negative to move up
        float bob = 0f;
        if (BobAmplitude > 0f && BobFrequency > 0f)
        {
            // Envelope curve (0 at start/end, peak at mid-walk)
            float envelope = 4f * t * (1f - t);
            bob = -Mathf.Sin(_walkElapsed * MathF.Tau * BobFrequency) * BobAmplitude * envelope;
        }

        // --- Final motion ---
        float x = Mathf.Lerp(_startPos.X, _targetPos.X, eased);
        int finalY = Mathf.RoundToInt(baseYNow + _offsetYComp + bob);

        WindowToMove.Position = new Vector2I(
            Mathf.RoundToInt(x),
            finalY
        );

        if (t >= 1f)
        {
            // Snap cleanly to the current base at the end of the animation
            WindowToMove.Position = new Vector2I(_targetPos.X, Mathf.RoundToInt(baseYNow));
            _offsetYComp = 0f;
            EnterIdle();
        }
    }
}
