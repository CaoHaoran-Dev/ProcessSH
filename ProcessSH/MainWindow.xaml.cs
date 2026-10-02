using System.Runtime.InteropServices;
using System.Windows.Input;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using ProcessSH.Models;
using ProcessSH.Services;
using ProcessSH.Views;
using WinRT.Interop;
using Windows.Graphics;

namespace ProcessSH;

public sealed partial class MainWindow : Window
{
    private AppWindow _appWindow;
    private bool _hideOnDeactivate;
    private bool _trayMenuOpen;
    private bool _suppressHide;

    private const int WindowWidth = 520;
    private const int BaseHeight = 200;
    private const int MaxHeight = 500;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int X,
        int Y,
        int cx,
        int cy,
        uint uFlags);

    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(IntPtr hwnd);

    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;

    public ICommand ToggleWindowCommand { get; }

    public MainWindow()
    {
        InitializeComponent();

        _appWindow = GetAppWindow();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        _appWindow.Resize(new SizeInt32(WindowWidth, BaseHeight));
        PositionWindowBottomLeft();
        _appWindow.IsShownInSwitchers = false;
        _appWindow.SetIcon("Assets/AppIcon.ico");

        if (_appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
            presenter.IsMaximizable = false;
            presenter.IsMinimizable = false;
        }

        RootFrame.Navigate(typeof(MainPage));

        _hideOnDeactivate = AppSettings.Current.HideOnDeactivate;
        Activated += OnWindowActivated;
        Closed += OnWindowClosed;

        ToggleWindowCommand = new RelayCommand(ToggleWindow);

        ApplyLocalization();
        Localization.LanguageChanged += ApplyLocalization;
    }

    private AppWindow GetAppWindow()
    {
        var hwnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        return AppWindow.GetFromWindowId(windowId);
    }

    private void ApplyLocalization()
    {
        TrayNewItem.Text = Localization.Get("tray.new");
        TrayToggleItem.Text = Localization.Get("tray.toggle");
        TrayClearItem.Text = Localization.Get("tray.clearhistory");
        TraySettingsItem.Text = Localization.Get("tray.settings");
        TrayAboutItem.Text = Localization.Get("tray.about");
        TrayQuitItem.Text = Localization.Get("tray.quit");
    }

    private void PositionWindowBottomLeft()
    {
        var displayArea = DisplayArea.GetFromWindowId(_appWindow.Id, DisplayAreaFallback.Primary);
        if (displayArea == null) return;

        var workArea = displayArea.WorkArea;
        var size = _appWindow.Size;
        int x = workArea.X + 20;
        int y = workArea.Y + workArea.Height - size.Height - 20;

        _appWindow.Move(new PointInt32(x, y));
    }

    /// <summary>根据输出行数动态调整窗口高度（使用 Win32 API 绕过 DPI Bug）</summary>
    public void ResizeForOutput(int lineCount)
    {
        int lineHeight = 20;
        int extra = Math.Max(0, lineCount - 1) * lineHeight;
        int newHeight = Math.Min(BaseHeight + extra, MaxHeight);

        if (newHeight == _appWindow.Size.Height) return;

        var displayArea = DisplayArea.GetFromWindowId(_appWindow.Id, DisplayAreaFallback.Primary);
        if (displayArea == null) return;

        var workArea = displayArea.WorkArea;
        int x = workArea.X + 20;
        int y = workArea.Y + workArea.Height - newHeight - 20;

        var hwnd = WindowNative.GetWindowHandle(this);
        uint dpi = GetDpiForWindow(hwnd);
        float scalingFactor = dpi / 96f;

        SetWindowPos(hwnd, IntPtr.Zero,
            x, y,
            (int)(WindowWidth * scalingFactor),
            (int)(newHeight * scalingFactor),
            SWP_NOZORDER | SWP_NOACTIVATE);
    }

    public void SuppressHide(bool suppress)
    {
        _suppressHide = suppress;
    }

    public void RefreshMainPage()
    {
        if (RootFrame.Content is MainPage page)
        {
            page.Refresh();
        }
    }

    private void OnWindowActivated(object sender, WindowActivatedEventArgs args)
    {
        if (!_hideOnDeactivate) return;
        if (_trayMenuOpen) return;
        if (_suppressHide) return;

        if (args.WindowActivationState == WindowActivationState.Deactivated)
        {
            DispatcherQueue.TryEnqueue(async () =>
            {
                await Task.Delay(100);

                if (_trayMenuOpen) return;
                if (_suppressHide) return;

                _appWindow.Hide();
            });
        }
    }

    private void TrayMenu_Opening(object sender, object e)
    {
        _trayMenuOpen = true;
    }

    private void TrayMenu_Closed(object sender, object e)
    {
        _trayMenuOpen = false;
    }

    private void OnWindowClosed(object sender, WindowEventArgs args)
    {
        args.Handled = true;
        _appWindow.Hide();
    }

    public void ShowWindow()
    {
        _appWindow.Show();
        Activate();

        DispatcherQueue.TryEnqueue(() =>
        {
            if (RootFrame.Content is MainPage page)
                page.FocusInput();
        });
    }

    public void HideWindow()
    {
        _appWindow.Hide();
    }

    public void ToggleWindow()
    {
        if (_appWindow.IsVisible)
            HideWindow();
        else
            ShowWindow();
    }

    private void TrayNewWindow_Click(object sender, RoutedEventArgs e)
    {
        var newWindow = new MainWindow();
        App.AllWindows.Add(newWindow);
        newWindow.Closed += (s, args) => App.AllWindows.Remove(newWindow);
        newWindow.Activate();
    }

    private void TrayToggleWindow_Click(object sender, RoutedEventArgs e)
    {
        ToggleWindow();
    }

    private void TrayClearHistory_Click(object sender, RoutedEventArgs e)
    {
        new CommandHistory().ClearAll();
    }

    private void TraySettings_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow();
        SuppressHide(true);

        settingsWindow.Closed += (s, args) =>
        {
            SuppressHide(false);
            RefreshMainPage();
        };

        settingsWindow.Activate();
    }

    private void TrayAbout_Click(object sender, RoutedEventArgs e)
    {
        var aboutWindow = new AboutWindow();
        SuppressHide(true);

        aboutWindow.Closed += (s, args) =>
        {
            SuppressHide(false);
        };

        aboutWindow.Activate();
    }

    private void TrayQuit_Click(object sender, RoutedEventArgs e)
    {
        Environment.Exit(0);
    }
}

internal sealed class RelayCommand : ICommand
{
    private readonly Action _action;

    public RelayCommand(Action action) => _action = action;

    event EventHandler? ICommand.CanExecuteChanged
    {
        add { }
        remove { }
    }

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter) => _action();
}