using Godot;

public partial class LogItem : PanelContainer
{
    public Label Label { get; private set; }

    public override void _Ready()
    {
        Label = GetNode<Label>("MarginContainer/Label");
    }

    public void SetText(string text)
    {
        Label.Text = text;
    }
}
