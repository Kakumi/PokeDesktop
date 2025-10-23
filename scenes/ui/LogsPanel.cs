using Godot;

public partial class LogsPanel : Control
{
    [Export] public PackedScene LogItem { get; set; }

    public Container Logs { get; private set; }
    public ScrollContainer ScrollContainer { get; private set; }

    public override void _Ready()
    {
        ScrollContainer = GetNode<ScrollContainer>("ScrollContainer");
        Logs = ScrollContainer.GetNode<Container>("Logs");

        Logger.Instance.LogReceived += Instance_LogReceived;

        foreach (var item in Logger.Instance.Logs)
        {
            Instance_LogReceived(item);
        }
    }

    private void Instance_LogReceived(string log)
    {
        var instance = LogItem.Instantiate<LogItem>();
        Logs.AddChild(instance);

        instance.SetText(log);
    }
}
