using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Logger : Node
{
    public static Logger Instance { get; private set; }

    public List<string> _logs = new List<string>();

    [Signal] public delegate void LogReceivedEventHandler(string log);

    public override void _Ready()
    {
    }

    public void Debug(string message)
    {
#if DEBUG
        GD.Print(message);
        AddToQueue(LogLevel.Debug, message);
#endif
    }

    public void Info(string message)
    {
        GD.Print(message);
        AddToQueue(LogLevel.Info, message);
    }

    public void Warning(string message)
    {
        GD.PushWarning(message);
        AddToQueue(LogLevel.Warning, message);
    }

    public void Error(string message, Exception exception = null)
    {
        GD.PrintErr(message, exception);
        AddToQueue(LogLevel.Error, message);

        if (exception != null)
        {
            AddToQueue(LogLevel.Error, exception.StackTrace);
        }
    }

    private void AddToQueue(LogLevel level, string message)
    {
        var log = $"[{level.ToString().ToUpper()}:{DateTime.Now.ToString("HH:mm:ss")}] {message}";
        _logs.Add(log);

        EmitSignal(SignalName.LogReceived, log);
    }

    public IReadOnlyList<string> Logs => _logs.ToList();

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
