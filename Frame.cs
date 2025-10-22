using Godot;
using PKHeX.Core;
using System;
using System.Runtime.InteropServices;

public partial class Frame : Node2D
{
    [Export] public int Height = 200;

    [Export] public PackedScene PokemonFrame;

    private bool _clickThough;

    public override void _Ready()
    {
        _clickThough = true;

        SetWindow();

        LoadPokemons();

        MakeFullyClickThrough_NonWindows(_clickThough);
        MakeFullyClickThrough_Windows(_clickThough);

        GetViewport().GuiEmbedSubwindows = false;

        //var window = new Window();
        //AddChild(window);
        //window.Visible = true;
        //window.Position = new Vector2I(1500, 500);
        //window.Title = "Salut";
        //window.Size = new Vector2I(300, 200);
        //window.CloseRequested += () =>
        //{
        //    GD.Print("close");
        //};
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed && !key.Echo && key.Keycode == Key.F1)
        {
            MakeFullyClickThrough_NonWindows(!_clickThough);
            MakeFullyClickThrough_Windows(!_clickThough);
            _clickThough = !_clickThough;
        }
    }

    private void SetWindow()
    {
        Vector2I screenSize = DisplayServer.ScreenGetSize();
        Rect2I usable = DisplayServer.ScreenGetUsableRect();
        var taskbarHeight = screenSize.Y - (usable.Position.Y + usable.Size.Y);

        Vector2I windowSize = new Vector2I(screenSize.X, Height);
        Vector2I windowPosition = new Vector2I(0, screenSize.Y - Height - taskbarHeight);

        //var region = _ui.GetRegion(windowPosition);
        //DisplayServer.WindowSetMousePassthrough(region);

        DisplayServer.WindowSetSize(windowSize);
        DisplayServer.WindowSetPosition(windowPosition);

        GD.Print($"Window positionned at {windowPosition}, size {windowSize}");
    }

    private void LoadPokemons()
    {
        var sav = SaveUtil.GetSaveFile(@"C:\Users\manga\Documents\PKHeX (25.08.30)a\TEST_SAVE.sav")
                  ?? throw new InvalidOperationException("Type de sauvegarde non reconnu.");

        int max = 0;
        for (int i = 0; i < max && i < sav.PartyCount; i++)
        {
            var pkm = sav.PartyData[i];
            var frame = CreatePokemonFrame(pkm);
        }
    }

    private PokemonFrame CreatePokemonFrame(PKM pokemon)
    {
        var instance = PokemonFrame.Instantiate<PokemonFrame>();
        AddChild(instance);

        //instance.Init(pokemon);

        return instance;
    }

    private void MakeFullyClickThrough_NonWindows(bool passthrough)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        if (passthrough)
        {
            var offscreen = new Vector2[] {
            new Vector2(-1000, -1000),
            new Vector2(-1001, -1000),
            new Vector2(-1000, -1001)
        };

            DisplayServer.WindowSetMousePassthrough(offscreen);
        }
        else
        {
            DisplayServer.WindowSetMousePassthrough(Array.Empty<Vector2>());
        }
    }

    private void MakeFullyClickThrough_Windows(bool passthrough)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            WinClickThrough.SetClickThrough(passthrough);
        }
    }
}

public static class WinClickThrough
{
    // --- Win32 ---
    const int GWL_EXSTYLE = -20;
    const int WS_EX_LAYERED = 0x00080000;
    const int WS_EX_TRANSPARENT = 0x00000020;

    const uint LWA_ALPHA = 0x02;

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

    static IntPtr GetHwnd(int windowId = 0)
    {
        // Handle natif de la fenêtre Godot
        return (IntPtr)DisplayServer.WindowGetNativeHandle(
            DisplayServer.HandleType.WindowHandle, windowId);
    }

    public static void SetClickThrough(bool enable, int windowId = 0)
    {
        var hwnd = GetHwnd(windowId);
        if (hwnd == IntPtr.Zero) return;

        long ex = GetWindowLongPtr(hwnd, GWL_EXSTYLE).ToInt64();

        if (enable)
        {
            ex |= (WS_EX_LAYERED | WS_EX_TRANSPARENT);
            SetWindowLongPtr(hwnd, GWL_EXSTYLE, new IntPtr(ex));

            // Optionnel : fixe l’opacité à 100% (assure l’affichage)
            SetLayeredWindowAttributes(hwnd, 0, 255, LWA_ALPHA);
        }
        else
        {
            ex &= ~(WS_EX_LAYERED | WS_EX_TRANSPARENT);
            SetWindowLongPtr(hwnd, GWL_EXSTYLE, new IntPtr(ex));
        }
    }

    public static void Toggle(int windowId = 0)
    {
        var hwnd = GetHwnd(windowId);
        if (hwnd == IntPtr.Zero) return;

        long ex = GetWindowLongPtr(hwnd, GWL_EXSTYLE).ToInt64();
        bool enabled = (ex & WS_EX_TRANSPARENT) != 0;
        SetClickThrough(!enabled, windowId);
    }
}