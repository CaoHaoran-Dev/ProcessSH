﻿using System.Collections.Generic;
using Microsoft.UI.Xaml;
using ProcessSH.Models;
using ProcessSH.Services;
using ProcessSH.Utils;

namespace ProcessSH;

public partial class App : Application
{
    private HotkeyManager? _hotkey;

    public static MainWindow? MainWindowInstance { get; private set; }

    /// <summary>所有打开的主窗口（单窗口模式下最多 1 个）</summary>
    public static List<MainWindow> AllWindows { get; } = new();

    /// <summary>所有跟踪的窗口（用于统一换材质）。</summary>
    public static List<Window> TrackedWindows { get; } = new();

    public static HotkeyManager? HotkeyInstance { get; private set; }

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var settings = AppSettings.Current;
        Localization.Current = settings.Language;

        var window = GetOrCreateMainWindow();

        if (!settings.HideWindowOnLaunch)
            window.Activate();
        else
            window.HideWindow();

        window.DispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                _hotkey = new HotkeyManager(window);
                HotkeyInstance = _hotkey;
                ApplyHotkey(settings.ToggleHotkey);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] Hotkey 初始化异常: {ex}");
            }
        });

        AppDomain.CurrentDomain.ProcessExit += (s, e) =>
        {
            _hotkey?.Dispose();
        };
    }

    public static MainWindow GetOrCreateMainWindow()
    {
        if (MainWindowInstance != null)
            return MainWindowInstance;

        var w = new MainWindow();
        MainWindowInstance = w;
        AllWindows.Add(w);
        return w;
    }

    public static bool ApplyHotkey(string hotkeyString)
    {
        if (HotkeyInstance == null || MainWindowInstance == null)
            return false;

        var parsed = HotkeyManager.Parse(hotkeyString);
        if (parsed == null) return false;

        var previous = AppSettings.Current.ToggleHotkey;

        var ok = HotkeyInstance.Register(
            parsed.Value.modifiers,
            parsed.Value.vk,
            () => MainWindowInstance?.ToggleWindow());

        if (!ok && !string.Equals(previous, hotkeyString, StringComparison.Ordinal))
        {
            var oldParsed = HotkeyManager.Parse(previous);
            if (oldParsed != null)
            {
                HotkeyInstance.Register(
                    oldParsed.Value.modifiers,
                    oldParsed.Value.vk,
                    () => MainWindowInstance?.ToggleWindow());
            }
        }

        return ok;
    }

    // ─── 窗口材质跟踪 ───

    /// <summary>
    /// 注册一个窗口。创建后立即应用当前材质，关闭时自动注销。
    /// </summary>
    public static void TrackWindow(Window window)
    {
        if (window == null) return;
        if (TrackedWindows.Contains(window)) return;

        TrackedWindows.Add(window);

        // 立即应用当前设置
        BackdropHelper.ApplyFromSettings(window);

        window.Closed += (_, _) =>
        {
            TrackedWindows.Remove(window);
        };
    }

    /// <summary>
    /// 把材质应用到所有跟踪的窗口 + 主窗口。
    /// </summary>
    public static void ApplyMaterialToAll(WindowMaterial material)
    {
        foreach (var w in TrackedWindows.ToList())
        {
            try { BackdropHelper.Apply(w, material); } catch { }
        }

        if (MainWindowInstance != null)
        {
            try { BackdropHelper.Apply(MainWindowInstance, material); } catch { }
        }
    }
}