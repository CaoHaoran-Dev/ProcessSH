using System.Collections.Generic;
using Microsoft.UI.Xaml;
using ProcessSH.Models;
using ProcessSH.Services;

namespace ProcessSH;

public partial class App : Application
{
    private MainWindow? _window;
    private HotkeyManager? _hotkey;

    public static MainWindow? MainWindowInstance { get; private set; }

    /// <summary>所有打开的 MainWindow，用于多窗口场景下定位宿主窗口</summary>
    public static List<MainWindow> AllWindows { get; } = new();

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        MainWindowInstance = _window;
        AllWindows.Add(_window);
        _window.Closed += (s, e) => AllWindows.Remove(_window);

        _window.Activate();

        _hotkey = new HotkeyManager(_window);
        _hotkey.Register(
            HotkeyManager.MOD_CONTROL | HotkeyManager.MOD_WIN,
            0x52,
            () => _window?.ToggleWindow());

        ApplyHotkey(AppSettings.Current.ToggleHotkey);

        AppDomain.CurrentDomain.ProcessExit += (s, e) =>
        {
            _hotkey?.Dispose();
        };
    }

    public static bool ApplyHotkey(string hotkeyString)
    {
        if (HotkeyInstance == null || MainWindowInstance == null) return false;

        var parsed = HotkeyManager.Parse(hotkeyString);
        if (parsed == null) return false;

        return HotkeyInstance.Register(
            parsed.Value.modifiers,
            parsed.Value.vk,
            () => MainWindowInstance?.ToggleWindow());
    }

    public static HotkeyManager? HotkeyInstance { get; private set; }
}