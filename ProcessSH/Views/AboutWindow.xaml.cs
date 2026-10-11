using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using ProcessSH.Models;
using WinRT.Interop;
using Windows.Graphics;

namespace ProcessSH.Views;

public sealed partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();

        // ✅ 交给 App 统一跟踪材质
        App.TrackWindow(this);

        var hwnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);

        appWindow.Resize(new SizeInt32(400, 340));
        TrySetIcon(appWindow);

        if (appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsMaximizable = false;
            presenter.IsMinimizable = false;
            presenter.IsResizable = false;
        }

        ApplyLocalization();
        Localization.LanguageChanged += ApplyLocalization;

        Closed += (s, e) =>
        {
            Localization.LanguageChanged -= ApplyLocalization;
        };

        RootFrame.Navigate(typeof(AboutPage));
    }

    private void ApplyLocalization()
    {
        Title = Localization.Get("about.title");
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