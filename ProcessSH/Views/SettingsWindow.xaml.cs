using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using ProcessSH.Models;
using WinRT.Interop;
using Windows.Graphics;

namespace ProcessSH.Views;

public sealed partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();

        // ✅ 交给 App 统一跟踪材质
        App.TrackWindow(this);

        var hwnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);

        appWindow.Resize(new SizeInt32(780, 640));
        TrySetIcon(appWindow);

        if (appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsMaximizable = false;
            presenter.IsMinimizable = false;
            presenter.IsResizable = true;
        }

        ApplyLocalization();
        Localization.LanguageChanged += ApplyLocalization;
        Closed += (s, e) =>
        {
            Localization.LanguageChanged -= ApplyLocalization;
        };

        RootFrame.Navigate(typeof(SettingsPage));
    }

    private void ApplyLocalization()
    {
        Title = Localization.Get("settings.title");
    }

    private static void TrySetIcon(AppWindow appWindow)
    {
        try
        {
            var icon = Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico");
            if (File.Exists(icon))
                appWindow.SetIcon(icon);
        }
        catch { }
    }
}