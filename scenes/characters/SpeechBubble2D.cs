using Godot;

public partial class SpeechBubble2D : Node2D
{
    [Export] public string Text;
    [Export] public HorizontalAlignment HorizontalAlignment = HorizontalAlignment.Center;
    [Export] public Font FileFont = ThemeDB.FallbackFont;
    [Export] public int FontSize = ThemeDB.FallbackFontSize;
    [Export] public Vector2 Offset = new(0, -40);

    [Export] public bool DrawBackground = true;
    [Export] public Color TextColor = new Color(1, 1, 1);
    [Export] public Color BgColor = new Color(0, 0, 0, 0.6f);
    [Export] public Color BorderColor = new Color(0, 0, 0, 0.9f);
    [Export] public Vector2 Padding = new(6, 4);

    public override void _Ready()
    {
        QueueRedraw();
    }

    public void SetText(string t) { Text = t; QueueRedraw(); }

    public Vector2 GetTextBox()
    {
        var textSize = FileFont.GetStringSize(Text, fontSize: FontSize);
        var ascent = FileFont.GetAscent(FontSize);
        var descent = FileFont.GetDescent(FontSize);

        return new Vector2(
            textSize.X + Padding.X * 2f,
            ascent + descent + Padding.Y * 2f
        );
    }

    public Rect2 GetLabelRect()
    {
        var rectSize = GetTextBox();
        var topLeft = new Vector2(-rectSize.X * 0.5f, -rectSize.Y) + Offset;

        return new Rect2(topLeft, rectSize);
    }

    public override void _Draw()
    {
        if (string.IsNullOrEmpty(Text) || FileFont == null || FontSize <= 0)
        {
            return;
        }

        if (DrawBackground)
        {
            var rect = GetLabelRect();
            DrawRect(rect, BgColor, filled: true);
            DrawPolyline(new Vector2[] {
                rect.Position,
                rect.Position + new Vector2(rect.Size.X, 0),
                rect.Position + rect.Size,
                rect.Position + new Vector2(0, rect.Size.Y),
                rect.Position
            }, BorderColor, 1.5f);
        }

        var textSize = FileFont.GetStringSize(Text, fontSize: FontSize);
        var descent = FileFont.GetDescent(FontSize);
        var baseline = new Vector2(-textSize.X * 0.5f, -(descent + Padding.Y)) + Offset;
        DrawString(FileFont, baseline, Text, HorizontalAlignment, -1, FontSize, TextColor);
    }
}
