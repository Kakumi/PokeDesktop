using Godot;
using H.NotifyIcon.Core;
using System.Drawing;

public partial class TrayManager : Node
{
    public override void _Ready()
    {
#pragma warning disable CA1416
#if GODOT_WINDOWS
        var path = ProjectSettings.GlobalizePath("res://app.ico");
        var _icon = new Icon(path);

        InitTrayIcon(_icon);
#endif
#pragma warning restore CA1416
    }

    private void InitTrayIcon(Icon icon)
    {
#pragma warning disable CA1416
        using var trayIcon = new TrayIconWithContextMenu
        {
            Icon = icon.Handle,
            ToolTip = "ToolTip",
        };

        trayIcon.ContextMenu = new H.NotifyIcon.Core.PopupMenu
        {
            Items =
                {
                    new PopupMenuItem("Settings", (_, _) => OnOpenSettings()),
                    new PopupMenuSeparator(),
                    new PopupMenuItem("Leave", (_, _) => OnQuit()),
                },
        };

        trayIcon.Create();
#pragma warning restore CA1416
    }

    private void OnOpenSettings()
    {
        GD.Print("Ouverture des paramètres !");
        // Ouvre ta fenêtre Godot des paramètres
        CallDeferred(nameof(OpenSettingsWindow));
    }

    private void OnQuit()
    {
        GetTree().Quit();
    }

    private void OpenSettingsWindow()
    {
        GD.Print("open settings");
        //var scene = GD.Load<PackedScene>("res://Scenes/SettingsWindow.tscn");
        //var window = scene.Instantiate<Window>();
        //GetTree().Root.AddChild(window);
        //window.PopupCentered(new Vector2I(400, 300));
    }
}