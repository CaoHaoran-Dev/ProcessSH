using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using ProcessSH.Models;
using WinRT.Interop;
using Windows.Graphics;

namespace ProcessSH.Views;

public sealed partial class HelpWindow : Window
{
    public HelpWindow()
    {
        InitializeComponent();

        // ✅ 交给 App 统一跟踪材质
        App.TrackWindow(this);

        var hwnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);

        appWindow.Resize(new SizeInt32(720, 520));
        TrySetIcon(appWindow);

        if (appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsMaximizable = false;
            presenter.IsMinimizable = true;
            presenter.IsResizable = true;
        }

        ApplyLocalization();
        Localization.LanguageChanged += ApplyLocalization;
        Closed += (_, _) => Localization.LanguageChanged -= ApplyLocalization;

        RootFrame.Navigate(typeof(HelpPage));
    }

    private void ApplyLocalization()
    {
        Title = Localization.Get("help.title");
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