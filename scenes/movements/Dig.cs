using Godot;
using System;

public partial class Dig : BaseMovement
{
    private enum Phase
    {
        Sink,        // go down below the screen
        Underground, // invisible: move horizontally while below
        Rise,        // come back up at target
        Done
    }

    // -------- Tunable parameters (adjust in editor if you want via [Export]) --------
    [Export] public float SinkDuration { get; set; } = 0.5f; // time to go down
    [Export] public float UndergroundMinHold { get; set; } = 0.5f; // optional pause underground
    [Export] public float UndergroundMaxHold { get; set; } = 2f; // optional pause underground
    [Export] public float RiseDuration { get; set; } = 0.5f; // time to come up
    [Export] public bool MoveWhileUnder { get; set; } = true;  // move X underground to target
    [Export] public float StretchYOnSink { get; set; } = 0.7f;  // vertical squash while digging in ( <1 )
    [Export] public float StretchYOnRise { get; set; } = 1.25f; // brief tall shape when emerging ( >1 )

    // “Volume-ish” compensation: wider when shorter, thinner when taller (simple look-good rule).
    private static float XForY(float yScale) => 1f / Mathf.Sqrt(Mathf.Max(0.0001f, yScale));

    // -------- Runtime state --------
    private Phase _phase;
    private float _phaseElapsed;

    // We’ll scale the sprite if present, else the window node itself.
    private TextureRect _nodeToScale;
    private Vector2 _baseScale;
    private Color _baseModulate;

    // Dynamic base tracking + anti-pop compensation (handles layout/size changes mid-animation).
    private float _baseYPrev;
    private float _offsetYComp;
    private float _undergroundHold;

    protected override void EnterCustomAnimation()
    {
        _state = State.Walking; // reuse "Walking" update loop from BaseMovement
        _phase = Phase.Sink;
        _phaseElapsed = 0f;

        _startPos = WindowToMove.Position;

        // Pick a horizontal distance like Walker does (random dir & dist).
        float dist = RandRange(WalkDistMin, WalkDistMax);
        _dirSign = _rng.Next(0, 2) == 0 ? -1f : 1f;
        var usable = WindowToMove.GetWindowUsable();
        float targetX = Mathf.Clamp(
            _startPos.X + _dirSign * dist,
            usable.X, usable.Y
        );

        //Calculate underground hold
        float ratio = Mathf.InverseLerp(WalkDistMin, WalkDistMax, dist);
        _undergroundHold = Mathf.Lerp(UndergroundMinHold, UndergroundMaxHold, ratio);

        _targetPos = new Vector2I((int)targetX, _startPos.Y); // Y will be set from base every frame

        // Choose what to scale (sprite if available, else the container).
        _nodeToScale = Instance?.PokemonSprite;
        _baseScale = _nodeToScale.Scale;
        _baseModulate = (_nodeToScale as CanvasItem)?.Modulate ?? Colors.White;

        // Init base tracking
        _baseYPrev = GetWinBaseY();
        _offsetYComp = 0f;

        // Optional: face direction of travel
        if (Instance?.PokemonSprite != null)
            Instance.PokemonSprite.FlipH = _dirSign > 0f;
    }

    protected override void UpdateCustomAnimation(float dt)
    {
        _phaseElapsed += dt;

        switch (_phase)
        {
            case Phase.Sink:
                UpdateSink(dt);
                break;
            case Phase.Underground:
                UpdateUnderground(dt);
                break;
            case Phase.Rise:
                UpdateRise(dt);
                break;
            case Phase.Done:
                EnterIdle();
                break;
        }
    }

    // --- Phase 1: go down below the screen with a squash and fade ---
    private void UpdateSink(float dt)
    {
        float baseYNow = UpdateDynamicBaseAndComp(dt);

        float t = Mathf.Clamp(_phaseElapsed / SinkDuration, 0f, 1f);
        float ease = EaseInCubic(t); // start gentle, end faster (digging down)

        // Compute Y target: base + depth (positive is downward in Godot 2D)
        float yBelow = baseYNow + WindowToMove.Size.Y;

        // Position: lerp from surface to below
        float x = _startPos.X;
        float y = Mathf.Lerp(baseYNow, yBelow, ease) + _offsetYComp;

        // Scale: vertical squash as we go down
        float yScale = Mathf.Lerp(1f, StretchYOnSink, ease);
        float xScale = Mathf.Lerp(1f, XForY(StretchYOnSink), ease);
        SetScale(xScale, yScale);

        // Fade out slightly while sinking (optional but sells the effect)
        float alpha = Mathf.Lerp(_baseModulate.A, 0.0f, EaseInCubic(t * 0.9f)); // mostly fade by end
        SetAlpha(alpha);

        WindowToMove.Position = new Vector2I(Mathf.RoundToInt(x), Mathf.RoundToInt(y));

        if (_phaseElapsed >= SinkDuration)
            AdvancePhase(Phase.Underground);
    }

    // --- Phase 2: invisible underground, optionally move horizontally to target ---
    private void UpdateUnderground(float dt)
    {
        float baseYNow = UpdateDynamicBaseAndComp(dt);

        // Stay below the surface while invisible
        float yBelow = baseYNow + WindowToMove.Size.Y + _offsetYComp;

        float x = WindowToMove.Position.X;
        if (MoveWhileUnder)
        {
            // Move X under the ground towards target during this phase
            float t = Mathf.Clamp(_phaseElapsed / Mathf.Max(0.001f, _undergroundHold), 0f, 1f);
            float eased = EaseInOut(t);
            x = Mathf.Lerp(_startPos.X, _targetPos.X, eased);
        }

        // Fully invisible underground
        SetAlpha(0f);
        // Keep a compact scale underground (neutral)
        SetScale(_baseScale.X, _baseScale.Y);

        WindowToMove.Position = new Vector2I(Mathf.RoundToInt(x), Mathf.RoundToInt(yBelow));

        if (_phaseElapsed >= _undergroundHold)
            AdvancePhase(Phase.Rise);
    }

    // --- Phase 3: come back up at the target with a brief tall stretch and fade in ---
    private void UpdateRise(float dt)
    {
        float baseYNow = UpdateDynamicBaseAndComp(dt);

        float t = Mathf.Clamp(_phaseElapsed / RiseDuration, 0f, 1f);
        float ease = EaseOutCubic(t); // emerge fast then settle

        // Start (underground) → surface
        float yBelow = baseYNow + WindowToMove.Size.Y;
        float y = Mathf.Lerp(yBelow, baseYNow, ease) + _offsetYComp;

        // X: at target (if we moved while under) else lerp quickly here
        float x;
        if (MoveWhileUnder)
        {
            x = _targetPos.X;
        }
        else
        {
            // If you didn't move X underground, finish the X move here
            x = Mathf.Lerp(_startPos.X, _targetPos.X, ease);
        }

        // Scale: brief tall stretch when emerging then back to normal
        float yScale = Mathf.Lerp(StretchYOnRise, 1f, ease);
        float xScale = Mathf.Lerp(XForY(StretchYOnRise), 1f, ease);
        SetScale(xScale, yScale);

        // Fade back in
        float alpha = Mathf.Lerp(0f, _baseModulate.A, EaseOutCubic(t));
        SetAlpha(alpha);

        WindowToMove.Position = new Vector2I(Mathf.RoundToInt(x), Mathf.RoundToInt(y));

        if (_phaseElapsed >= RiseDuration)
        {
            // Restore exact visuals and finish
            SetScale(_baseScale.X, _baseScale.Y);
            SetAlpha(_baseModulate.A);
            AdvancePhase(Phase.Done);
        }
    }

    // -------- Helpers --------

    // Dynamic base tracking + anti-pop compensation.
    // If GetWinBaseY() jumps (layout resize, e.g., 900→800), we instantly cancel the jump
    // via _offsetYComp, then smoothly decay it to 0 (no visual snap).
    private float UpdateDynamicBaseAndComp(float dt)
    {
        float baseYNow = GetWinBaseY();
        float deltaBase = baseYNow - _baseYPrev;

        if (Mathf.Abs(deltaBase) > 0.5f)
        {
            _offsetYComp -= deltaBase; // cancel the visual jump this frame
        }
        _baseYPrev = baseYNow;

        // Smoothly reduce the compensation
        float k = 12f; // ~120ms half-life; lower = smoother, higher = snappier
        float alpha = 1f - Mathf.Exp(-k * dt);
        _offsetYComp = Mathf.Lerp(_offsetYComp, 0f, alpha);

        return baseYNow;
    }

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

    private static float EaseInCubic(float t) => t * t * t;
    private static float EaseOutCubic(float t) => 1f - MathF.Pow(1f - t, 3f);
}
