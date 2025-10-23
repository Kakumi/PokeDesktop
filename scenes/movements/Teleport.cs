using Godot;
using System;

public partial class Teleport : BaseMovement
{
    [Export] public float StretchDuration { get; set; } = 0.18f;
    [Export] public float FadeOutDuration { get; set; } = 0.08f;
    [Export] public float FadeInDuration { get; set; } = 0.14f;

    [Export] public float StretchY { get; set; } = 1.8f;  // vertical stretch at source
    [Export] public float SquashY { get; set; } = 0.6f;  // vertical squash at destination (brief)

    private enum Phase
    {
        StretchOut,   // stretch at source
        FadeOut,      // fade to invisible
        Jump,         // instant position change
        FadeIn,       // fade back in with inverse squash
        Done
    }

    // ---- Tunable parameters ----
    // Keep “fake volume conservation”: wider when short, thinner when tall.
    // For simplicity we invert around 1.0 (not physically correct but looks nice).
    private static float XForY(float yScale) => 1f / Mathf.Sqrt(Mathf.Max(0.0001f, yScale));

    // ---- Runtime state ----
    private Phase _phase;
    private float _phaseElapsed;

    private TextureRect _nodeToScale;     // sprite if available, else WindowToMove
    private Vector2 _baseScale;      // original scale of node
    private Color _baseModulate;     // original color (to restore alpha)

    protected override void EnterCustomAnimation()
    {
        _state = State.Walking;
        _phase = Phase.StretchOut;
        _phaseElapsed = 0f;

        _startPos = WindowToMove.Position;
        float dist = RandRange(WalkDistMin, WalkDistMax);
        _dirSign = _rng.Next(0, 2) == 0 ? -1f : 1f;

        // Horizontal target; Y follows the dynamic base each frame (no drift if height changes).
        var usable = WindowToMove.GetWindowUsable();
        float targetX = Mathf.Clamp(_startPos.X + _dirSign * dist, usable.X, usable.Y);
        // Target Y is initial base; we’ll override with current base at the jump moment.
        _targetPos = new Vector2I((int)targetX, (int)GetWinBaseY());

        // Choose what to scale: the main sprite if present, else the container node.
        _nodeToScale = Instance?.PokemonSprite;
        _baseScale = _nodeToScale.Scale;
        _baseModulate = (_nodeToScale as CanvasItem)?.Modulate ?? Colors.White;

        // Optional: face the destination horizontally.
        if (Instance?.PokemonSprite != null)
            Instance.PokemonSprite.FlipH = _dirSign > 0f;
    }

    protected override void UpdateCustomAnimation(float dt)
    {
        _phaseElapsed += dt;

        switch (_phase)
        {
            case Phase.StretchOut:
                UpdateStretchOut(dt);
                break;

            case Phase.FadeOut:
                UpdateFadeOut(dt);
                break;

            case Phase.Jump:
                DoJumpInstant();
                break;

            case Phase.FadeIn:
                UpdateFadeIn(dt);
                break;

            case Phase.Done:
                EnterIdle();
                break;
        }
    }

    // --- Phase 1: vertical stretch at the source position ---
    private void UpdateStretchOut(float dt)
    {
        // Read dynamic base Y in case the component height changes.
        float baseY = GetWinBaseY();
        WindowToMove.Position = new Vector2I((int)_startPos.X, (int)baseY);

        float t = Mathf.Clamp(_phaseElapsed / StretchDuration, 0f, 1f);
        float eased = EaseOutCubic(t);

        float y = Mathf.Lerp(1f, StretchY, eased);
        float x = Mathf.Lerp(1f, XForY(StretchY), eased);
        SetScale(x, y);

        if (_phaseElapsed >= StretchDuration)
            AdvancePhase(Phase.FadeOut);
    }

    // --- Phase 2: fade out to invisible (still stretched) ---
    private void UpdateFadeOut(float dt)
    {
        float baseY = GetWinBaseY();
        WindowToMove.Position = new Vector2I((int)_startPos.X, (int)baseY);

        float t = Mathf.Clamp(_phaseElapsed / FadeOutDuration, 0f, 1f);
        float alpha = Mathf.Lerp(_baseModulate.A, 0f, t);
        SetAlpha(alpha);

        if (_phaseElapsed >= FadeOutDuration)
            AdvancePhase(Phase.Jump);
    }

    // --- Phase 3: instant teleport (reposition while invisible) ---
    private void DoJumpInstant()
    {
        // Snap target Y to CURRENT base to avoid any offset due to layout change.
        float baseY = GetWinBaseY();

        // Set position instantly
        WindowToMove.Position = new Vector2I((int)_targetPos.X, (int)baseY);

        // Prepare the inverse squash for the arrival (short + wide)
        float y = SquashY;
        float x = XForY(SquashY);
        SetScale(x, y);

        // Remain invisible for the single frame of the jump
        SetAlpha(0f);

        AdvancePhase(Phase.FadeIn);
    }

    // --- Phase 4: fade in + recover to normal scale with a little overshoot feel ---
    private void UpdateFadeIn(float dt)
    {
        float baseY = GetWinBaseY();
        WindowToMove.Position = new Vector2I((int)_targetPos.X, (int)baseY);

        float t = Mathf.Clamp(_phaseElapsed / FadeInDuration, 0f, 1f);

        // Fade back to original alpha with a smooth ease
        float alpha = Mathf.Lerp(0f, _baseModulate.A, EaseInCubic(t));
        SetAlpha(alpha);

        // Scale back to (1,1) with a tiny ease-out
        float y = Mathf.Lerp(SquashY, 1f, EaseOutCubic(t));
        float x = Mathf.Lerp(XForY(SquashY), 1f, EaseOutCubic(t));
        SetScale(x, y);

        if (_phaseElapsed >= FadeInDuration)
        {
            // Ensure exact restoration
            SetScale(1f, 1f);
            SetAlpha(_baseModulate.A);

            AdvancePhase(Phase.Done);
        }
    }

    // --- Helpers ---
    private void AdvancePhase(Phase next)
    {
        _phase = next;
        _phaseElapsed = 0f;
    }

    private void SetScale(float x, float y)
    {
        if (_nodeToScale != null)
            _nodeToScale.Scale = new Vector2(x, y);
    }

    private void SetAlpha(float a)
    {
        if (_nodeToScale is CanvasItem ci)
            ci.Modulate = new Color(ci.Modulate.R, ci.Modulate.G, ci.Modulate.B, a);
    }

    // Simple ease helpers (feel free to reuse your own EaseInOut)
    private static float EaseOutCubic(float t) => 1f - MathF.Pow(1f - t, 3f);
    private static float EaseInCubic(float t) => t * t * t;
}
